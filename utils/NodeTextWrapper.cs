using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MgAl2O4.Utils
{
    // see source for details:
    // https://github.com/aers/FFXIVClientStructs/issues/1040

    public unsafe class NodeTextWrapper
    {
        private byte* strBuffer = null;
        private nuint bufferLen = 0;

        public NodeTextWrapper() { }
        public NodeTextWrapper(string text)
        {
            Set(text);
        }

        public byte* Get() => strBuffer;

        public void Set(string text)
        {
            int strLen = Encoding.UTF8.GetByteCount(text); // get length of string as UTF-8 bytes
            nuint newBufferLen = (nuint)(strLen + 1); // need one extra byte for the null terminator

            if (strBuffer == null || newBufferLen > bufferLen) // reallocate buffer if it doesn't already exist or is too small
            {
                NativeMemory.Free(strBuffer);

                strBuffer = (byte*)NativeMemory.Alloc(newBufferLen);
                bufferLen = newBufferLen;
            }

            Span<byte> bufferSpan = new(strBuffer, (int)newBufferLen); // wrap buffer in a span so you can use GetBytes
            Encoding.UTF8.GetBytes(text, bufferSpan); // convert string to UTF-8 and store in your buffer
            bufferSpan[strLen] = 0; // add null terminator to the end of your string
        }

        public void Free()
        {
            NativeMemory.Free(strBuffer);
            strBuffer = null;
            bufferLen = 0;
        }
    }
}
