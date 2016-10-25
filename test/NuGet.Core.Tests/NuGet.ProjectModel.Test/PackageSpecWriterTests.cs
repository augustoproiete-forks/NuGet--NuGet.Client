// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using Xunit;

namespace NuGet.ProjectModel.Test
{
    public class PackageSpecWriterTests
    {
        private static readonly PackageSpec _emptyPackageSpec = JsonPackageSpecReader.GetPackageSpec(new JObject());

        [Fact]
        public void Write_ThrowsForNullPackageSpec()
        {
            var writer = new JsonWriter();

            Assert.Throws<ArgumentNullException>(() => PackageSpecWriter.Write(packageSpec: null, writer: writer));
        }

        [Fact]
        public void Write_ThrowsForNullWriter()
        {
            Assert.Throws<ArgumentNullException>(() => PackageSpecWriter.Write(_emptyPackageSpec, writer: null));
        }

        [Fact]
        public void Write_ReadWriteDependencies()
        {
            // Arrange
            var json = @"{
  ""title"": ""My Title"",
  ""version"": ""1.2.3"",
  ""description"": ""test"",
  ""authors"": [
    ""author1"",
    ""author2""
  ],
  ""copyright"": ""2016"",
  ""language"": ""en-US"",
  ""packInclude"": {
    ""file"": ""file.txt""
  },
  ""packOptions"": {
    ""owners"": [
      ""owner1"",
      ""owner2""
    ],
    ""tags"": [
      ""tag1"",
      ""tag2""
    ],
    ""projectUrl"": ""http://my.url.com"",
    ""iconUrl"": ""http://my.url.com"",
    ""summary"": ""Sum"",
    ""releaseNotes"": ""release noted"",
    ""licenseUrl"": ""http://my.url.com""
  },
  ""scripts"": {
    ""script1"": [
      ""script.js""
    ]
  },
  ""dependencies"": {
    ""packageA"": {
      ""suppressParent"": ""All"",
      ""target"": ""Project""
    }
  },
  ""frameworks"": {
    ""net46"": {}
  }
}";
            // Act & Assert
            VerifyJsonPackageSpecRoundTrip(json);
        }

        [Fact]
        public void Write_ReadWriteSinglePackageType()
        {
            // Arrange
            var json = @"{
  ""packOptions"": {
    ""packageType"": ""DotNetTool""
  }
}";

            // Act & Assert
            VerifyJsonPackageSpecRoundTrip(json);
        }

        [Fact]
        public void Write_ReadWriteMultiplePackageType()
        {
            // Arrange
            var json = @"{
  ""packOptions"": {
    ""packageType"": [
      ""Dependency"",
      ""DotNetTool""
    ]
  }
}";

            // Act & Assert
            VerifyJsonPackageSpecRoundTrip(json);
        }

        [Fact]
        public void WriteToFile_ThrowsForNullPackageSpec()
        {
            Assert.Throws<ArgumentNullException>(() => PackageSpecWriter.WriteToFile(packageSpec: null, filePath: @"C:\a.json"));
        }

        [Fact]
        public void WriteToFile_ThrowsForNullFilePath()
        {
            Assert.Throws<ArgumentException>(() => PackageSpecWriter.WriteToFile(_emptyPackageSpec, filePath: null));
        }

        [Fact]
        public void WriteToFile_ThrowsForEmptyFilePath()
        {
            Assert.Throws<ArgumentException>(() => PackageSpecWriter.WriteToFile(_emptyPackageSpec, filePath: null));
        }

        private static void VerifyJsonPackageSpecRoundTrip(string json)
        {
            // Arrange & Act
            var spec = JsonPackageSpecReader.GetPackageSpec(json, "testName", @"C:\fake\path");

            var writer = new JsonWriter();
            PackageSpecWriter.Write(spec, writer);

            var actualResult = writer.GetJson();

            // Assert
            Assert.Equal(json, actualResult);
        }
    }
}