using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace MSMQCheck
{

    class Program
    {
        static void PrintQueueList(List<string> queues)
        {
            if (queues.Count == 0)
            {
                Console.WriteLine("(none)");
            }
            else
            {
                foreach (string queue in queues)
                {
                    Console.WriteLine(queue);
                }
            }
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("MSMQCheck v0.1.4");
            Console.WriteLine("Fetching queue names from WMI...");
            string[] queues;
            try
            {
                queues = MessageQueueHelpers.GetQueues();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get queue names. Exception: {ex.Message}");
                return;
            }
            Console.WriteLine($"Found {queues.Length} queues.");

            // used to store a list of queues that have some security issue.
            var unauthenticatedQueues = new List<string>();
            var anonymousWritableQueues = new List<string>();
            var anonymousReadableQueues = new List<string>();
            var anonymousModifiableQueues = new List<string>();
            var everyoneWritableQueues = new List<string>();
            var everyoneReadableQueues = new List<string>();
            var everyoneModifiableQueues = new List<string>();
            var insecureOwnerQueues = new List<string>();
            var multicastAccessibleQueues = new List<string>();
            var disabledEncryptionQueues = new List<string>();
            var optionalEncryptionQueues = new List<string>();

            // enumerate all queues and do the checks
            foreach (string queue in queues)
            {
                Console.WriteLine($"Queue: {queue}");
                string formatName = MessageQueueHelpers.TranslateQueueNameToFormat(queue);
                Console.WriteLine($"Format Name: {formatName}");

                try
                {
                    bool auth = MessageQueueSecurity.IsQueueAuthenticated(formatName);
                    Console.WriteLine($" > Authenticated: {auth}");
                    if (!auth)
                    {
                        unauthenticatedQueues.Add(queue);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while getting queue authenticated property: {ex}");
                }

                try
                {
                    var privacy = MessageQueueSecurity.GetQueuePrivacyLevel(formatName);
                    Console.WriteLine($" > Privacy Level: {privacy}");
                    if (privacy == QueuePrivacyLevel.MQ_PRIV_LEVEL_NONE)
                    {
                        disabledEncryptionQueues.Add(queue);
                    }
                    else if (privacy == QueuePrivacyLevel.MQ_PRIV_LEVEL_OPTIONAL)
                    {
                        optionalEncryptionQueues.Add(queue);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while getting queue privacy level property: {ex}");
                }

                try
                {
                    var multicast = MessageQueueSecurity.GetMulticastAddress(formatName);
                    if (multicast != null)
                    {
                        Console.WriteLine($" > Multicast: Yes (address = {multicast})");
                        multicastAccessibleQueues.Add(queue);
                    }
                    else
                    {
                        Console.WriteLine($" > Multicast: No");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while getting queue multicast property: {ex}");
                }

                try
                {
                    var security = MessageQueueSecurity.GetQueueSecurity(formatName);

                    try
                    {
                        var account = security.Owner.Translate(typeof(NTAccount)) as NTAccount;
                        if (account != null)
                        {
                            Console.WriteLine($" > Owner: {account.ToString()}");
                            if (security.Owner.IsWellKnown(WellKnownSidType.AnonymousSid) ||
                                security.Owner.IsWellKnown(WellKnownSidType.BuiltinGuestsSid))
                            {
                                insecureOwnerQueues.Add(queue);
                            }
                        }
                        else
                        {
                            Console.WriteLine($" > Owner: {security.Owner.ToString()}");
                        }
                    }
                    catch
                    {
                        if (security.Owner == null)
                        {
                            Console.WriteLine($" > Owner: [null]");
                        }
                        else
                        {
                            Console.WriteLine($" > Owner: {security.Owner.ToString()}");
                        }
                    }
                    
                    if (security.DiscretionaryAcl != null)
                    {
                        foreach (CommonAce ace in security.DiscretionaryAcl)
                        {
                            var account = ace.SecurityIdentifier.Translate(typeof(NTAccount)) as NTAccount;
                            var accessMask = (NativeMethods.MQ_QUEUE_ACCESS_MASK)ace.AccessMask;
                            Console.WriteLine($" > ACL Entry: {account.ToString()} {ace.AceQualifier.ToString()} {accessMask.ToString()}");

                            if (ace.AceQualifier == AceQualifier.AccessAllowed && ace.SecurityIdentifier.IsWellKnown(WellKnownSidType.AnonymousSid))
                            {
                                if (accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.RECEIVE_MESSAGE) ||
                                    accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.RECEIVE_JOURNAL_MESSAGE) ||
                                    accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.PEEK_MESSAGE))
                                {
                                    anonymousReadableQueues.Add(queue);
                                }
                                if (accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.WRITE_MESSAGE))
                                {
                                    anonymousWritableQueues.Add(queue);
                                }
                                if (accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.TAKE_QUEUE_OWNERSHIP) ||
                                    accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.CHANGE_QUEUE_PERMISSIONS) ||
                                    accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.SET_QUEUE_PROPERTIES))
                                {
                                    anonymousModifiableQueues.Add(queue);
                                }
                            }

                            if (ace.AceQualifier == AceQualifier.AccessAllowed && (
                                    ace.SecurityIdentifier.IsWellKnown(WellKnownSidType.WorldSid) ||
                                    ace.SecurityIdentifier.IsWellKnown(WellKnownSidType.AccountDomainUsersSid) ||
                                    ace.SecurityIdentifier.IsWellKnown(WellKnownSidType.AuthenticatedUserSid)
                                    ))
                            {
                                if (accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.RECEIVE_MESSAGE) ||
                                    accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.RECEIVE_JOURNAL_MESSAGE) ||
                                    accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.PEEK_MESSAGE))
                                {
                                    everyoneReadableQueues.Add(queue);
                                }
                                if (accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.WRITE_MESSAGE))
                                {
                                    everyoneWritableQueues.Add(queue);
                                }
                                if (accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.TAKE_QUEUE_OWNERSHIP) ||
                                    accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.CHANGE_QUEUE_PERMISSIONS) ||
                                    accessMask.HasFlag(NativeMethods.MQ_QUEUE_ACCESS_MASK.SET_QUEUE_PROPERTIES))
                                {
                                    everyoneModifiableQueues.Add(queue);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while getting queue security: {ex.Message}");
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("---- Report ----");
            Console.WriteLine();
            Console.WriteLine("Authentication disabled (ACL ignored):");
            PrintQueueList(unauthenticatedQueues);
            Console.WriteLine("Insecure owner (guest or anonymous):");
            PrintQueueList(insecureOwnerQueues);
            Console.WriteLine("Multicast accessible:");
            PrintQueueList(multicastAccessibleQueues);
            Console.WriteLine("Network encryption disallowed:");
            PrintQueueList(disabledEncryptionQueues);
            Console.WriteLine("Network encryption optional (default but insecure):");
            PrintQueueList(optionalEncryptionQueues);
            Console.WriteLine("Anonymous user can write messages to queue:");
            PrintQueueList(anonymousWritableQueues);
            Console.WriteLine("Anonymous user can receive/peek messages from queue:");
            PrintQueueList(anonymousReadableQueues);
            Console.WriteLine("Anonymous user can change the ACL or queue properties:");
            PrintQueueList(anonymousModifiableQueues);
            Console.WriteLine("Insecure user group (everyone, authenticated users, etc.) can write messages to queue:");
            PrintQueueList(everyoneWritableQueues);
            Console.WriteLine("Insecure user group (everyone, authenticated users, etc.) can receive/peek messages from queue:");
            PrintQueueList(everyoneReadableQueues);
            Console.WriteLine("Insecure user group (everyone, authenticated users, etc.) can change the ACL or queue properties:");
            PrintQueueList(everyoneModifiableQueues);

            Console.WriteLine();
            Console.WriteLine("----------------");
            Console.WriteLine("Done.");
        }
    }
}
