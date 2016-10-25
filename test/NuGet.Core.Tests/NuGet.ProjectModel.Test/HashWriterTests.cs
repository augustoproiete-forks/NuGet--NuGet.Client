// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace NuGet.ProjectModel.Test
{
    public class HashWriterTests : IDisposable
    {
        private readonly IHashFunction _hashFunc;
        private readonly HashWriter _writer;

        public HashWriterTests()
        {
            _hashFunc = new Sha512HashFunction();
            _writer = new HashWriter(_hashFunc);
        }

        public void Dispose()
        {
            _writer.Dispose();
            _hashFunc.Dispose();
        }

        [Fact]
        public void Constructor_ThrowsForNullHashFunc()
        {
            Assert.Throws<ArgumentNullException>(() => new HashWriter(hashFunc: null));
        }

        [Fact]
        public void GetHash_HasDefaultValue()
        {
            const string expectedHash = "z4PhNX7vuL3xVChQ1m2AB9Yg5AULVxXcg/SpIdNs6c5H0NE8XYXysP+DGNKHfuwvY7kxvUdBeoGlODJ6+SfaPg==";
            var actualHash = _writer.GetHash();

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void GetHash_ComputesOverEntireObject()
        {
            _writer.WriteObjectStart("a");
            _writer.WriteNameValue("b", 0);
            _writer.WriteNameValue("c", "d");
            _writer.WriteNameArray("e", new[] { "f", "g" });
            _writer.WriteObjectEnd();

            const string expectedHash = "+cbiB6wM9JcGB60IhxKr6VYXLC0bwr4/3u3aX1Z8m1UB3XTU+uGHA5NG56RA2OpKa5YNx3Jrrj4cbnt8ucrrUw==";
            var actualHash = _writer.GetHash();

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void WriteObjectStart_ThrowsForNullName()
        {
            Assert.Throws<ArgumentNullException>(() => _writer.WriteObjectStart(name: null));
        }

        [Fact]
        public void WriteObjectStart_ThrowsIfReadOnly()
        {
            MakeReadOnly();

            Assert.Throws<InvalidOperationException>(() => _writer.WriteObjectStart("a"));
        }

        [Fact]
        public void WriteObjectStart_SupportsEmptyName()
        {
            _writer.WriteObjectStart(name: "");
            _writer.WriteObjectEnd();

            const string expectedHash = "Pkux5n7tdf8w4+XzErvaXVx06XnacoeFmxa3EH8c0xPVXd1o4sDdgAWB86ifBaqzEgb9yCPWR5TQ3wyo4ENrBw==";
            var actualHash = _writer.GetHash();

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void WriteObjectEnd_ThrowsIfReadOnly()
        {
            _writer.WriteObjectStart("a");

            MakeReadOnly();

            Assert.Throws<InvalidOperationException>(() => _writer.WriteObjectEnd());
        }

        [Fact]
        public void WriteObjectEnd_ThrowsIfCalledOnRoot()
        {
            Assert.Throws<InvalidOperationException>(() => _writer.WriteObjectEnd());
        }

        [Fact]
        public void WriteNameValue_WithIntValue_ThrowsForNullName()
        {
            Assert.Throws<ArgumentNullException>(() => _writer.WriteNameValue(name: null, value: 0));
        }

        [Fact]
        public void WriteNameValue_WithIntValue_ThrowsIfReadOnly()
        {
            MakeReadOnly();

            Assert.Throws<InvalidOperationException>(() => _writer.WriteNameValue("a", value: 1));
        }

        [Fact]
        public void WriteNameValue_WithIntValue_SupportsEmptyName()
        {
            _writer.WriteNameValue(name: "", value: 3);

            const string expectedHash = "3UonDw6IeP0q7BYNiUKgTfOawoxaFTNOspWyeHa3yhIXJ0F/PXBLcQNz/Ycmpu9IEmJGy3SXglCU7wanFoIjzQ==";
            var actualHash = _writer.GetHash();

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void WriteNameValue_WithStringValue_ThrowsForNullName()
        {
            Assert.Throws<ArgumentNullException>(() => _writer.WriteNameValue(name: null, value: "a"));
        }

        [Fact]
        public void WriteNameValue_WithStringValue_ThrowsIfReadOnly()
        {
            MakeReadOnly();

            Assert.Throws<InvalidOperationException>(() => _writer.WriteNameValue("a", "b"));
        }

        [Fact]
        public void WriteNameValue_WithStringValue_SupportsEmptyNameAndEmptyValue()
        {
            _writer.WriteNameValue(name: "", value: "");

            const string expectedHash = "PI9IdNQHcprUKZ9FGde3Nog0QeqfWRA1XyLVquluf+9p+RPYNivEI4SuwF9lMwhJ8h0yYrpl2eZbuPp8nGi7pg==";
            var actualHash = _writer.GetHash();

            Assert.Equal(expectedHash, actualHash);
        }

        [Fact]
        public void WriteNameArray_ThrowsForNullName()
        {
            Assert.Throws<ArgumentNullException>(() => _writer.WriteNameArray(name: null, values: new[] { "a" }));
        }

        [Fact]
        public void WriteNameArray_ThrowsIfReadOnly()
        {
            MakeReadOnly();

            Assert.Throws<InvalidOperationException>(() => _writer.WriteNameArray("a", new[] { "b" }));
        }

        [Fact]
        public void WriteNameArray_SupportsEmptyNameAndEmptyValues()
        {
            _writer.WriteNameArray(name: "", values: Enumerable.Empty<string>());

            const string expectedHash = "UbQXYQqZBQimhCwrfdh5JxNlcnmd8ztS0HCD7fpXMgvH+GJf218j7+0UsF/X0a09iOhRYgAqD8I8wsspGR11Iw==";
            var actualHash = _writer.GetHash();

            Assert.Equal(expectedHash, actualHash);
        }

        private void MakeReadOnly()
        {
            _writer.GetHash();
        }
    }
}