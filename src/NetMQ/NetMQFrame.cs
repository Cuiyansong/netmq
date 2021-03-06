﻿using System;
using System.Text;
using JetBrains.Annotations;

namespace NetMQ
{
    /// <summary>
    /// Objects of class NetMQFrame serve to hold a Buffer (that consists of a byte-array containing a unit of a message-queue message)
    /// and provide methods to construct it given a string and an encoding.
    /// </summary>
    public class NetMQFrame : IEquatable<NetMQFrame>, IEquatable<byte[]>
    {
        /// <summary>
        /// This is the length of the byte-array data buffer.
        /// </summary>
        private int m_messageSize;

        /// <summary>
        /// This holds the computed hash-code for this object.
        /// </summary>
        private int m_hash;

        /// <summary>
        /// Create a new NetMQFrame containing the given byte-array data.
        /// </summary>
        /// <param name="buffer">a byte-array to hold as the frame's data</param>
        public NetMQFrame([CanBeNull] byte[] buffer)
        {
            if (buffer == null)
            {
                buffer = new byte[0];
            }

            Buffer = buffer;
            MessageSize = buffer.Length;
        }

        /// <summary>
        /// Create a new NetMQFrame containing the given string-message,
        /// using the default ASCII encoding.
        /// </summary>
        /// <param name="message">a string containing the message-data of the frame</param>
        public NetMQFrame([NotNull] string message)
            : this(Encoding.ASCII.GetBytes(message))
        {}

        /// <summary>
        /// Create a new NetMQFrame containing the given string-message,
        /// using the given encoding to convert it into a byte-array.
        /// </summary>
        /// <param name="message">a string containing the message-data of the frame</param>
        /// <param name="encoding">the Encoding to use to convert the given string-message into the internal byte-array</param>
        public NetMQFrame([NotNull] string message, [NotNull] Encoding encoding)
            : this(encoding.GetBytes(message))
        {}

        /// <summary>
        /// Create a new NetMQFrame with a data-buffer pre-sised to the given length.
        /// </summary>
        /// <param name="length">the number of bytes to allocate for the data-buffer</param>
        public NetMQFrame(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", "A non-negative value is expected.");
            }

            Buffer = new byte[length];
            MessageSize = length;
        }

        /// <summary>
        /// Get or set the size of the message data contained in the frame, which here represents the number of bytes.
        /// </summary>
        public int MessageSize
        {
            get { return m_messageSize; }
            set
            {
                if (value < 0 || value > BufferSize)
                {
                    throw new ArgumentOutOfRangeException("value", "Expecting a non-negative value less than or equal to the buffer size.");
                }

                m_messageSize = value;
            }
        }

        /// <summary>
        /// Get the underlying frame-data buffer, which is an array of bytes.
        /// </summary>       
        [NotNull]
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// Get the maximum size of the frame-data buffer (ie, the number of bytes of the array).
        /// </summary>
        public int BufferSize
        {
            get { return Buffer.Length; }
        }

        /// <summary>
        /// Get a new empty <see cref="NetMQFrame"/> that may be used as message separators.
        /// </summary>
        public static NetMQFrame Empty
        {
            get { return new NetMQFrame(0); }
        }

        /// <summary>
        /// Get whether this NetMQFrame is empty - that is, has a Buffer of zero-length.
        /// </summary>
        public bool IsEmpty
        {
            get { return MessageSize == 0; }
        }

        /// <summary>
        /// Create and return a new NetMQFrame with a copy of the supplied byte-array buffer.
        /// </summary>
        /// <param name="buffer">the byte-array to copy into the new NetMQFrame</param>
        /// <returns>a new <see cref="NetMQFrame"/> containing a copy of the supplied byte-array</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        [NotNull]
        public static NetMQFrame Copy([NotNull] byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            var copy = new NetMQFrame(buffer.Length);

            System.Buffer.BlockCopy(buffer, 0, copy.Buffer, 0, buffer.Length);

            return copy;
        }

        /// <summary>
        /// Return this frame's data-buffer converted into a String, using the default ASCII encoding.
        /// </summary>
        /// <returns>the data buffer converted to a string</returns>
        [NotNull]
        public string ConvertToString()
        {
            return Encoding.ASCII.GetString(Buffer, 0, MessageSize);
        }

        /// <summary>
        /// Return this frame's data-buffer converted into a String using the given encoding.
        /// </summary>
        /// <param name="encoding">the Encoding to use to convert the internal byte-array buffer into a String</param>
        /// <returns>the data buffer converted to a string</returns>
        [NotNull]
        public string ConvertToString([NotNull] Encoding encoding)
        {
            return encoding.GetString(Buffer, 0, MessageSize);
        }

        /// <summary>
        /// Convert the buffer to integer in network byte order (big-endian)
        /// </summary>
        /// <returns></returns>
        public int ConvertToInt32()
        {
            return NetworkOrderBitsConverter.ToInt32(Buffer);
        }

        /// <summary>
        /// Convert the buffer to long in network byte order (big-endian)
        /// </summary>
        /// <returns></returns>
        public long ConvertToInt64()
        {
            return NetworkOrderBitsConverter.ToInt64(Buffer);
        }

        /// <summary>
        /// Create a deep-copy of the supplied <see cref="NetMQFrame"/>.
        /// </summary>
        /// <param name="frame">the <see cref="NetMQFrame"/> to copy</param>
        /// <returns>a <see cref="NetMQFrame"/> containing a copy of <paramref name="frame"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="frame"/> is null.</exception>
        [NotNull]
        public static NetMQFrame Copy([NotNull] NetMQFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException("frame");
            }

            var copy = new NetMQFrame(new byte[frame.BufferSize]);
            copy.MessageSize = frame.MessageSize;

            System.Buffer.BlockCopy(frame.Buffer, 0, copy.Buffer, 0, frame.BufferSize);

            return copy;
        }

        /// <summary>
        /// Create a deep-copy of this NetMQFrame and return it.
        /// </summary>
        /// <returns>a new NetMQFrame containing a copy of this one's buffer data</returns>
        [NotNull]
        public NetMQFrame Duplicate()
        {
            return Copy(this);
        }

        /// <summary>
        /// Return true if the buffer of this NetMQFrame is equal to the given byte-array.
        /// </summary>
        /// <param name="other">a byte-array buffer to compare this frame against</param>
        /// <returns></returns>
        public bool Equals([CanBeNull] byte[] other)
        {
            if (other == null)
                return false;

            if (other.Length != MessageSize)
                return false;

            if (ReferenceEquals(Buffer, other))
                return true;

            for (int i = 0; i < MessageSize; i++)
            {
                if (Buffer[i] != other[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determine whether the specified <see cref="NetMQFrame"/> is equal to the current <see cref="NetMQFrame"/>.
        /// </summary>
        /// <param name="other">the <see cref="NetMQFrame"/> to compare with the current <see cref="NetMQFrame"/>.</param>
        /// <returns>true if the specified NetMQFrame is equal to this one; otherwise, false</returns>
        public bool Equals([CanBeNull] NetMQFrame other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (MessageSize > other.BufferSize || MessageSize != other.MessageSize)
            {
                return false;
            }

            for (int i = 0; i < MessageSize; i++)
            {
                if (Buffer[i] != other.Buffer[i])
                {
                    return false;
                }
            }

            return true;
        }

        bool IEquatable<NetMQFrame>.Equals(NetMQFrame other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Return true if the given Object is a NetMQFrame which has a Buffer that is identical to that of this one.
        /// </summary>
        /// <param name="obj">the Object to compare this to</param>
        /// <returns>true only if the given Object is a NetMQFrame equal to this one</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as NetMQFrame);
        }

        /// <summary>
        /// Override the Object.GetHashCode method to return a hash-code derived from the content of the Buffer.
        /// That is only computed the first time this method is called.
        /// </summary>
        /// <returns>an integer that represents the computed hash-code</returns>
        public override int GetHashCode()
        {
            if (m_hash == 0)
            {
                foreach (var b in Buffer)
                {
                    m_hash = 31*m_hash + b;
                }
            }

            return m_hash;
        }

        /// <summary>
        /// Return an array of bytes that carries the content of this NetMQFrames Buffer.
        /// </summary>
        /// <param name="copy">if this argument is true - a new copy is made if BufferSize is equal to MessageSize</param>
        /// <returns>the Buffer as a byte-array, either newly-allocated or else (if copy is false) simply a reference to the actual Buffer</returns>
        [NotNull]
        public byte[] ToByteArray(bool copy = false)
        {
            if (!copy || MessageSize == BufferSize)
            {
                return Buffer;
            }

            var byteArray = new byte[MessageSize];

            System.Buffer.BlockCopy(Buffer, 0, byteArray, 0, MessageSize);

            return byteArray;
        }
    }
}
