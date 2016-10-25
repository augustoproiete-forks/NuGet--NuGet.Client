// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using NuGet.Common;

namespace NuGet.ProjectModel
{
    /// <summary>
    /// Generates a hash from an object graph.
    /// 
    /// This is non-private only to facilitate unit testing.
    /// </summary>
    public sealed class HashWriter : IObjectWriter, IDisposable
    {
        private const int _defaultBufferSize = 4096;

        // These delimiters are important in preserving the structure of an object map.
        // Although the values were chosen out of familiarly with JSON, other values could be used.
        private static readonly byte[] _null = new[] { (byte)0 };
        private static readonly byte[] _nameValueSeparator = new[] { (byte)':' };
        private static readonly byte[] _objectStart = new[] { (byte)'{' };
        private static readonly byte[] _objectEnd = new[] { (byte)'}' };
        private static readonly byte[] _stringStart = new[] { (byte)'"' };
        private static readonly byte[] _stringEnd = new[] { (byte)'"' };
        private static readonly byte[] _arrayStart = new[] { (byte)'[' };
        private static readonly byte[] _arrayEnd = new[] { (byte)']' };
        private static readonly byte[] _separator = new[] { (byte)',' };

        private readonly byte[] _buffer;
        private readonly IHashFunction _hashFunc;
        private bool _isReadOnly;
        private int _nestLevel;
        private int _position;

        /// <summary>
        /// Creates a new instance with the provide hash function.
        /// </summary>
        /// <param name="hashFunc">An <see cref="IHashFunction"/> instance.  Throws if <c>null</c>.</param>
        public HashWriter(IHashFunction hashFunc)
        {
            if (hashFunc == null)
            {
                throw new ArgumentNullException(nameof(hashFunc));
            }

            _hashFunc = hashFunc;
            _buffer = new byte[_defaultBufferSize];
        }

        public void Dispose()
        {
            _hashFunc.Dispose();
        }

        public void WriteObjectStart(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            ThrowIfReadOnly();

            Write(name);
            Write(_nameValueSeparator);
            Write(_objectStart);

            ++_nestLevel;
        }

        public void WriteObjectEnd()
        {
            ThrowIfReadOnly();

            if (_nestLevel == 0)
            {
                throw new InvalidOperationException();
            }

            Write(_objectEnd);
            Write(_separator);

            --_nestLevel;
        }

        public void WriteNameValue(string name, int value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            ThrowIfReadOnly();

            Write(name);
            Write(_nameValueSeparator);
            Write(BitConverter.GetBytes(value));
            Write(_separator);
        }

        public void WriteNameValue(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            ThrowIfReadOnly();

            Write(name);
            Write(_nameValueSeparator);
            Write(value);
            Write(_separator);
        }

        public void WriteNameArray(string name, IEnumerable<string> values)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            ThrowIfReadOnly();

            Write(name);
            Write(_nameValueSeparator);

            if (values == null)
            {
                Write(_null);
            }
            else
            {
                Write(_arrayStart);

                foreach (var value in values)
                {
                    Write(value);
                    Write(_separator);
                }

                Write(_arrayEnd);
            }

            Write(_separator);
        }

        /// <summary>
        /// Gets the hash for the object.
        ///
        /// Once GetHash is called, no further writing is allowed.
        /// </summary>
        /// <returns>A hash of the object.</returns>
        public string GetHash()
        {
            _isReadOnly = true;

            Flush();

            return _hashFunc.GetHash();
        }

        private void FlushIfFull()
        {
            if (_position == _buffer.Length)
            {
                Flush();
            }
        }

        private void Flush()
        {
            if (_position > 0)
            {
                _hashFunc.Update(_buffer, offset: 0, count: _position);

                _position = 0;
            }
        }

        private void Write(string value)
        {
            if (value == null)
            {
                Write(_null);
            }
            else
            {
                Write(_stringStart);
                Write(Encoding.UTF8.GetBytes(value));
                Write(_stringEnd);
            }
        }

        private void Write(byte[] bytes)
        {
            int bytesWritten = 0;

            while (bytesWritten < bytes.Length)
            {
                int bytesToWrite = Math.Min(_buffer.Length - _position, bytes.Length - bytesWritten);

                Buffer.BlockCopy(bytes, bytesWritten, _buffer, _position, bytesToWrite);

                _position += bytesToWrite;
                bytesWritten += bytesToWrite;

                FlushIfFull();
            }
        }

        private void ThrowIfReadOnly()
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException();
            }
        }
    }
}