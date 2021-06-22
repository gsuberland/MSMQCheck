# MSMQCheck

MSMQCheck is a security analysis tool for [Microsoft Message Queuing (MSMQ)](https://en.wikipedia.org/wiki/Microsoft_Message_Queuing). It dumps security information about each queue on the local system and generates a summary report.

The following issues are currently reported:

- **Queues with authentication disabled.** When authentication is disabled, the access control list (ACL) is ignored and everyone has full access to the queue.
- **Queues owned by ANONYMOUS LOGON or Guest.** Queues owned by these accounts are insecure, for obvious reasons.
- **Queues accessible via multicast addresses.** Multicast accessible queues are not inherently secure, but it is important to identify queues that are accessible via multicast due to the potential for evading firewall rules or accessing queues when the direct TCP/OS endpoints are disabled.
- **Queues with encryption disabled/optional.** Disabled means that encrypted connections are not supported, even if the client requests it. Optional means that unencrypted connections are allowed, and this is the default.
- **Queues that allow messages to be written by ANONYMOUS LOGON.** This allows anyone on the network to write messages to the queue.
- **Queues that allow messages to be received or peeked by ANONYMOUS LOGON.** This allows anyone on the network to read messages from the queue.
- **Queues that allow for queue properties to be modified by ANONYMOUS LOGON.** This allows anyone on the network to change the properties (either the queue properties or ACL) of the queue.
- **Queues that allow messages to be written by Everyone / Authenticated Users / Domain Users.** This allows anyone with a valid user account to write messages to the queue.
- **Queues that allow messages to be received or peeked by Everyone / Authenticated Users / Domain Users.** This allows anyone with a valid user account to read messages from the queue.
- **Queues that allow for queue properties to be modified by Everyone / Authenticated Users / Domain Users.** This allows anyone with a valid user account to change the properties (either the queue properties or ACL) of the queue.

For completeness, ACL issues are reported regardless of whether authentication is enabled. Just keep in mind that if authentication is disabled then it doesn't matter what the ACL says.

As a side note, it's worth mentioning the (functionally meaningless) distinction between private & public queues. A public queue's name can be enumerated remotely in a domain. A private queue's name cannot. That's the totality of the difference. A private queue has exactly the same functionality and accessibility as a public queue. Knowledge of a name is not a meaningful security boundary, so MSMQCheck does not report on it. You can tell whether the queue is private by looking to see if it has `\private$\` in its name or format name.

## Example output

```
MSMQCheck v0.1.4
Fetching queue names from WMI...
Found 8 queues.
Queue: mqtestbox\private$\fastpass
Format Name: DIRECT=OS:.\private$\fastpass
 > Authenticated: False
 > Privacy Level: MQ_PRIV_LEVEL_OPTIONAL
 > Multicast: No
 > Owner: mqtestbox\testuser
 > ACL Entry: Everyone AccessAllowed WRITE_MESSAGE, GET_QUEUE_PROPERTIES, GET_QUEUE_PERMISSIONS
 > ACL Entry: NT AUTHORITY\ANONYMOUS LOGON AccessAllowed WRITE_MESSAGE
 > ACL Entry: mqtestbox\testuser AccessAllowed DELETE_MESSAGE, PEEK_MESSAGE, WRITE_MESSAGE, DELETE_JOURNAL_MESSAGE, SET_QUEUE_PROPERTIES, GET_QUEUE_PROPERTIES

Queue: mqtestbox\private$\nationalpastime
Format Name: DIRECT=OS:.\private$\nationalpastime
 > Authenticated: False
 > Privacy Level: MQ_PRIV_LEVEL_OPTIONAL
 > Multicast: No
 > Owner: mqtestbox\testuser
 > ACL Entry: mqtestbox\testuser AccessAllowed DELETE_MESSAGE, PEEK_MESSAGE, WRITE_MESSAGE, DELETE_JOURNAL_MESSAGE, SET_QUEUE_PROPERTIES, GET_QUEUE_PROPERTIES
 > ACL Entry: Everyone AccessAllowed DELETE_MESSAGE, PEEK_MESSAGE, WRITE_MESSAGE, DELETE_JOURNAL_MESSAGE, GET_QUEUE_PROPERTIES, GET_QUEUE_PERMISSIONS
 > ACL Entry: NT AUTHORITY\ANONYMOUS LOGON AccessAllowed WRITE_MESSAGE

Queue: mqtestbox\private$\snooker
Format Name: DIRECT=OS:.\private$\snooker
 > Authenticated: False
 > Privacy Level: MQ_PRIV_LEVEL_OPTIONAL
 > Multicast: No
 > Owner: mqtestbox\testuser
 > ACL Entry: mqtestbox\testuser AccessAllowed DELETE_MESSAGE, PEEK_MESSAGE, WRITE_MESSAGE, DELETE_JOURNAL_MESSAGE, SET_QUEUE_PROPERTIES, GET_QUEUE_PROPERTIES
 > ACL Entry: Everyone AccessAllowed DELETE_MESSAGE, PEEK_MESSAGE, WRITE_MESSAGE, DELETE_JOURNAL_MESSAGE, GET_QUEUE_PROPERTIES, GET_QUEUE_PERMISSIONS
 > ACL Entry: NT AUTHORITY\ANONYMOUS LOGON AccessAllowed WRITE_MESSAGE

Queue: mqtestbox\private$\gettothebackofthe
Format Name: DIRECT=OS:.\private$\gettothebackofthe
 > Authenticated: False
 > Privacy Level: MQ_PRIV_LEVEL_OPTIONAL
 > Multicast: No
 > Owner: mqtestbox\testuser
 > ACL Entry: mqtestbox\testuser AccessAllowed DELETE_MESSAGE, PEEK_MESSAGE, WRITE_MESSAGE, DELETE_JOURNAL_MESSAGE, SET_QUEUE_PROPERTIES, GET_QUEUE_PROPERTIES
 > ACL Entry: Everyone AccessAllowed DELETE_MESSAGE, PEEK_MESSAGE, WRITE_MESSAGE, DELETE_JOURNAL_MESSAGE, GET_QUEUE_PROPERTIES, GET_QUEUE_PERMISSIONS
 > ACL Entry: NT AUTHORITY\ANONYMOUS LOGON AccessAllowed WRITE_MESSAGE

Queue: mqtestbox\private$\dontjumpthe
Format Name: DIRECT=OS:.\private$\dontjumpthe
 > Authenticated: False
 > Privacy Level: MQ_PRIV_LEVEL_OPTIONAL
 > Multicast: No
 > Owner: mqtestbox\testuser
 > ACL Entry: mqtestbox\testuser AccessAllowed DELETE_MESSAGE, PEEK_MESSAGE, WRITE_MESSAGE, DELETE_JOURNAL_MESSAGE, SET_QUEUE_PROPERTIES, GET_QUEUE_PROPERTIES
 > ACL Entry: Everyone AccessAllowed DELETE_MESSAGE, PEEK_MESSAGE, WRITE_MESSAGE, DELETE_JOURNAL_MESSAGE, GET_QUEUE_PROPERTIES, GET_QUEUE_PERMISSIONS
 > ACL Entry: NT AUTHORITY\ANONYMOUS LOGON AccessAllowed WRITE_MESSAGE, PEEK_MESSAGE

Queue: mqtestbox\private$\order_queue$
Format Name: DIRECT=OS:.\private$\order_queue$
Exception while getting queue authenticated property: System.Exception: MQGetQueueProperties call failed. Status: -1072824283 (0xC00E0025)
   at MSMQCheck.MessageQueueSecurity.IsQueueAuthenticated(String formatName)
   at MSMQCheck.Program.Main(String[] args)
Exception while getting queue privacy level property: System.Exception: MQGetQueueProperties call failed. Status: -1072824283 (0xC00E0025)
   at MSMQCheck.MessageQueueSecurity.GetQueuePrivacyLevel(String formatName)
   at MSMQCheck.Program.Main(String[] args)
Exception while getting queue multicast property: System.Exception: MQGetQueueProperties call failed. Status: -1072824283 (0xC00E0025)
   at MSMQCheck.MessageQueueSecurity.GetMulticastAddress(String formatName)
   at MSMQCheck.Program.Main(String[] args)
Exception while getting queue security: MQGetQueueSecurity call failed. Result: 3222143013

Queue: mqtestbox\private$\admin_queue$
Format Name: DIRECT=OS:.\private$\admin_queue$
Exception while getting queue authenticated property: System.Exception: MQGetQueueProperties call failed. Status: -1072824283 (0xC00E0025)
   at MSMQCheck.MessageQueueSecurity.IsQueueAuthenticated(String formatName)
   at MSMQCheck.Program.Main(String[] args)
Exception while getting queue privacy level property: System.Exception: MQGetQueueProperties call failed. Status: -1072824283 (0xC00E0025)
   at MSMQCheck.MessageQueueSecurity.GetQueuePrivacyLevel(String formatName)
   at MSMQCheck.Program.Main(String[] args)
Exception while getting queue multicast property: System.Exception: MQGetQueueProperties call failed. Status: -1072824283 (0xC00E0025)
   at MSMQCheck.MessageQueueSecurity.GetMulticastAddress(String formatName)
   at MSMQCheck.Program.Main(String[] args)
Exception while getting queue security: MQGetQueueSecurity call failed. Result: 3222143013

Queue: Computer Queues
Format Name: Computer Queues
Exception while getting queue authenticated property: System.Exception: MQGetQueueProperties call failed. Status: -1072824290 (0xC00E001E)
   at MSMQCheck.MessageQueueSecurity.IsQueueAuthenticated(String formatName)
   at MSMQCheck.Program.Main(String[] args)
Exception while getting queue privacy level property: System.Exception: MQGetQueueProperties call failed. Status: -1072824290 (0xC00E001E)
   at MSMQCheck.MessageQueueSecurity.GetQueuePrivacyLevel(String formatName)
   at MSMQCheck.Program.Main(String[] args)
Exception while getting queue multicast property: System.Exception: MQGetQueueProperties call failed. Status: -1072824290 (0xC00E001E)
   at MSMQCheck.MessageQueueSecurity.GetMulticastAddress(String formatName)
   at MSMQCheck.Program.Main(String[] args)
Exception while getting queue security: MQGetQueueSecurity call failed. Result: 3222143006


---- Report ----

Authentication disabled (ACL ignored):
fastpass
nationalpastime
snooker
gettothebackofthe
dontjumpthe

Insecure owner (guest or anonymous):
(none)

Multicast accessible:
(none)

Network encryption disallowed:
(none)

Network encryption optional (default but insecure):
fastpass
nationalpastime
snooker
gettothebackofthe
dontjumpthe

Anonymous user can write messages to queue:
fastpass
nationalpastime
snooker
gettothebackofthe
dontjumpthe

Anonymous user can receive/peek messages from queue:
dontjumpthe

Anonymous user can change the ACL or queue properties:
(none)

Insecure user group (everyone, authenticated users, etc.) can write messages to queue:
fastpass
nationalpastime
snooker
gettothebackofthe
dontjumpthe

Insecure user group (everyone, authenticated users, etc.) can receive/peek messages from queue:
fastpass
nationalpastime
snooker
gettothebackofthe
dontjumpthe

Insecure user group (everyone, authenticated users, etc.) can change the ACL or queue properties:
(none)

----------------
Done.
```

## Troubleshooting

### Failed to get queue names

If you receive the error `Failed to get queue names. Exception: Invalid class`, this means you don't have MSMQ installed on the local system. MSMQCheck should be executed on the server that hosts the queues, not on a client system that accesses the queues remotely.

### MQGetQueueProperties call failed

When enumerating the MSMQ queues on the local system, the results include remote queues that are currently connected or have pending messages. MSMQCheck will attempt to query these remote queues, but in general you'll see this error when it tries. This is either because the protocol being used doesn't support this type of query (e.g. HTTP) or you don't have the rights to access queue information remotely. To get information about these queues, run MSMQCheck on the system that hosts them.

The local system may also include results such as "Computer Queues" or queues ending in `$`, at the end of the list. These are pretty much guaranteed to throw errors when MSMQCheck tries to query them. These are not normal queues and can generally be ignored.

## License

This project is [released under MIT License](LICENSE.md).

