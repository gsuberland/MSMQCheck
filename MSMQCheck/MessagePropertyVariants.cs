using System;
using System.Runtime.InteropServices;

namespace MSMQCheck
{
    [StructLayout(LayoutKind.Explicit)]
    struct MQPROPVARIANTS
    {
        [FieldOffset(0)]
        public short vt;

        [FieldOffset(2)]
        public short wReserved1;

        [FieldOffset(4)]
        public short wReserved2;

        [FieldOffset(6)]
        public short wReserved3;

        [FieldOffset(8)]
        public byte bVal;

        [FieldOffset(8)]
        public short iVal;

        [FieldOffset(8)]
        public int lVal;

        [FieldOffset(8)]
        public long hVal;

        [FieldOffset(8)]
        public IntPtr ptr;

        [FieldOffset(8)]
        public MQPROPVARIANTS.CAUB caub;

        public struct CAUB
        {
            public uint cElems;

            public IntPtr pElems;
        }
    }

    class QueuePropertyVariants : MessagePropertyVariants
    {
        private const int MaxQueuePropertyIndex = 26;

        public QueuePropertyVariants() : base(26, 101)
        {
        }
    }

    class MessagePropertyVariants
    {
        private const short VT_UNDEFINED = 0;

        public const short VT_EMPTY = 32767;

        public const short VT_ARRAY = 8192;

        public const short VT_BOOL = 11;

        public const short VT_BSTR = 8;

        public const short VT_CLSID = 72;

        public const short VT_CY = 6;

        public const short VT_DATE = 7;

        public const short VT_I1 = 16;

        public const short VT_I2 = 2;

        public const short VT_I4 = 3;

        public const short VT_I8 = 20;

        public const short VT_LPSTR = 30;

        public const short VT_LPWSTR = 31;

        public const short VT_NULL = 1;

        public const short VT_R4 = 4;

        public const short VT_R8 = 5;

        public const short VT_STREAMED_OBJECT = 68;

        public const short VT_STORED_OBJECT = 69;

        public const short VT_UI1 = 17;

        public const short VT_UI2 = 18;

        public const short VT_UI4 = 19;

        public const short VT_UI8 = 21;

        public const short VT_VECTOR = 4096;

        private int MAX_PROPERTIES = 61;

        private int basePropertyId = 1;

        private int propertyCount;

        private GCHandle handleVectorProperties;

        private GCHandle handleVectorIdentifiers;

        private GCHandle handleVectorStatus;

        private MessagePropertyVariants.MQPROPS reference;

        private int[] vectorIdentifiers;

        private int[] vectorStatus;

        private MQPROPVARIANTS[] vectorProperties;

        private short[] variantTypes;

        private object[] objects;

        private object[] handles;

        public MessagePropertyVariants(int maxProperties, int baseId)
        {
            this.reference = new MessagePropertyVariants.MQPROPS();
            this.MAX_PROPERTIES = maxProperties;
            this.basePropertyId = baseId;
            this.variantTypes = new short[this.MAX_PROPERTIES];
            this.objects = new object[this.MAX_PROPERTIES];
            this.handles = new object[this.MAX_PROPERTIES];
        }

        public MessagePropertyVariants()
        {
            this.reference = new MessagePropertyVariants.MQPROPS();
            this.variantTypes = new short[this.MAX_PROPERTIES];
            this.objects = new object[this.MAX_PROPERTIES];
            this.handles = new object[this.MAX_PROPERTIES];
        }

        public virtual void AdjustSize(int propertyId, int size)
        {
            this.handles[propertyId - this.basePropertyId] = (uint)size;
        }

        public byte[] GetGuid(int propertyId)
        {
            return (byte[])this.objects[propertyId - this.basePropertyId];
        }

        public short GetI2(int propertyId)
        {
            return (short)this.objects[propertyId - this.basePropertyId];
        }

        public int GetI4(int propertyId)
        {
            return (int)this.objects[propertyId - this.basePropertyId];
        }

        public IntPtr GetIntPtr(int propertyId)
        {
            object obj = this.objects[propertyId - this.basePropertyId];
            if (obj.GetType() != typeof(IntPtr))
            {
                return IntPtr.Zero;
            }
            return (IntPtr)obj;
        }

        public byte[] GetString(int propertyId)
        {
            return (byte[])this.objects[propertyId - this.basePropertyId];
        }

        public IntPtr GetStringVectorBasePointer(int propertyId)
        {
            return (IntPtr)this.handles[propertyId - this.basePropertyId];
        }

        public uint GetStringVectorLength(int propertyId)
        {
            return (uint)this.objects[propertyId - this.basePropertyId];
        }

        public byte GetUI1(int propertyId)
        {
            return (byte)this.objects[propertyId - this.basePropertyId];
        }

        public byte[] GetUI1Vector(int propertyId)
        {
            return (byte[])this.objects[propertyId - this.basePropertyId];
        }

        public short GetUI2(int propertyId)
        {
            return (short)this.objects[propertyId - this.basePropertyId];
        }

        public int GetUI4(int propertyId)
        {
            return (int)this.objects[propertyId - this.basePropertyId];
        }

        public long GetUI8(int propertyId)
        {
            return (long)this.objects[propertyId - this.basePropertyId];
        }

        public virtual void Ghost(int propertyId)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] != 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 0;
                this.propertyCount--;
            }
        }

        public virtual MessagePropertyVariants.MQPROPS Lock()
        {
            int[] numArray = new int[this.propertyCount];
            int[] numArray1 = new int[this.propertyCount];
            MQPROPVARIANTS[] length = new MQPROPVARIANTS[this.propertyCount];
            int num = 0;
            for (int i = 0; i < this.MAX_PROPERTIES; i++)
            {
                short num1 = this.variantTypes[i];
                if (num1 != 0)
                {
                    numArray[num] = i + this.basePropertyId;
                    length[num].vt = num1;
                    if (num1 == 4113)
                    {
                        if (this.handles[i] != null)
                        {
                            length[num].caub.cElems = (uint)this.handles[i];
                        }
                        else
                        {
                            length[num].caub.cElems = (uint)((byte[])this.objects[i]).Length;
                        }
                        GCHandle gCHandle = GCHandle.Alloc(this.objects[i], GCHandleType.Pinned);
                        this.handles[i] = gCHandle;
                        length[num].caub.pElems = gCHandle.AddrOfPinnedObject();
                    }
                    else if (num1 == 17 || num1 == 16)
                    {
                        length[num].bVal = (byte)this.objects[i];
                    }
                    else if (num1 == 18 || num1 == 2)
                    {
                        length[num].iVal = (short)this.objects[i];
                    }
                    else if (num1 == 19 || num1 == 3)
                    {
                        length[num].lVal = (int)this.objects[i];
                    }
                    else if (num1 == 21 || num1 == 20)
                    {
                        length[num].hVal = (long)this.objects[i];
                    }
                    else if (num1 == 31 || num1 == 72)
                    {
                        GCHandle gCHandle1 = GCHandle.Alloc(this.objects[i], GCHandleType.Pinned);
                        this.handles[i] = gCHandle1;
                        length[num].ptr = gCHandle1.AddrOfPinnedObject();
                    }
                    else if (num1 == 32767)
                    {
                        length[num].vt = 0;
                    }
                    num++;
                    if (this.propertyCount == num)
                    {
                        break;
                    }
                }
            }
            this.handleVectorIdentifiers = GCHandle.Alloc(numArray, GCHandleType.Pinned);
            this.handleVectorProperties = GCHandle.Alloc(length, GCHandleType.Pinned);
            this.handleVectorStatus = GCHandle.Alloc(numArray1, GCHandleType.Pinned);
            this.vectorIdentifiers = numArray;
            this.vectorStatus = numArray1;
            this.vectorProperties = length;
            this.reference.propertyCount = this.propertyCount;
            this.reference.propertyIdentifiers = this.handleVectorIdentifiers.AddrOfPinnedObject();
            this.reference.propertyValues = this.handleVectorProperties.AddrOfPinnedObject();
            this.reference.status = this.handleVectorStatus.AddrOfPinnedObject();
            return this.reference;
        }

        public virtual void Remove(int propertyId)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] != 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 0;
                this.objects[propertyId - this.basePropertyId] = null;
                this.handles[propertyId - this.basePropertyId] = null;
                this.propertyCount--;
            }
        }

        public virtual void SetEmpty(int propertyId)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 32767;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = null;
        }

        public void SetGuid(int propertyId, byte[] value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 72;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetI2(int propertyId, short value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 2;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetI4(int propertyId, int value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 3;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public virtual void SetNull(int propertyId)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 1;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = null;
        }

        public void SetString(int propertyId, byte[] value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 31;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetUI1(int propertyId, byte value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 17;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetUI1Vector(int propertyId, byte[] value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 4113;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetUI2(int propertyId, short value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 18;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetUI4(int propertyId, int value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 19;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public void SetUI8(int propertyId, long value)
        {
            if (this.variantTypes[propertyId - this.basePropertyId] == 0)
            {
                this.variantTypes[propertyId - this.basePropertyId] = 21;
                this.propertyCount++;
            }
            this.objects[propertyId - this.basePropertyId] = value;
        }

        public virtual void Unlock()
        {
            for (int i = 0; i < (int)this.vectorIdentifiers.Length; i++)
            {
                short num = this.vectorProperties[i].vt;
                if (this.variantTypes[this.vectorIdentifiers[i] - this.basePropertyId] == 1)
                {
                    if (num == 4113 || num == 1)
                    {
                        this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i].caub.cElems;
                    }
                    else if (num != 4127)
                    {
                        this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i].ptr;
                    }
                    else
                    {
                        this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i * 4].caub.cElems;
                        this.handles[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i * 4].caub.pElems;
                    }
                }
                else if (num == 31 || num == 72 || num == 4113)
                {
                    GCHandle gCHandle = (GCHandle)this.handles[this.vectorIdentifiers[i] - this.basePropertyId];
                    gCHandle.Free();
                    this.handles[this.vectorIdentifiers[i] - this.basePropertyId] = null;
                }
                else if (num == 17 || num == 16)
                {
                    this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i].bVal;
                }
                else if (num == 18 || num == 2)
                {
                    this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i].iVal;
                }
                else if (num == 19 || num == 3)
                {
                    this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i].lVal;
                }
                else if (num == 21 || num == 20)
                {
                    this.objects[this.vectorIdentifiers[i] - this.basePropertyId] = this.vectorProperties[i].hVal;
                }
            }
            this.handleVectorIdentifiers.Free();
            this.handleVectorProperties.Free();
            this.handleVectorStatus.Free();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MQPROPS
        {
            public int propertyCount;
            public IntPtr propertyIdentifiers;
            public IntPtr propertyValues;
            public IntPtr status;
        }
    }
}