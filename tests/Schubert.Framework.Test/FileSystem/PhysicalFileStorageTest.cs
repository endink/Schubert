using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schubert.Framework.FileSystem;
using System.IO;
using Xunit;
using System.Text;

namespace Schubert.Framework.Test.FileSystem
{
    public class PhysicalFileStorageTest
    {
        private PhysicalFileStorage CreateFileStorage()
        {
            var storage = new PhysicalFileStorage("A-scope", new DefaultFileRequestMapping("/Test"));
            return storage;
        }

        [Fact]
        private async Task CreateFileAsyncTest()
        {
            var s = this.CreateFileStorage();
            string content = "Hellow!";
            var bytes = Encoding.UTF8.GetBytes(content);
            using (MemoryStream ms = new MemoryStream())
            {
                await s.CreateFileAsync("Test/Test.txt", new MemoryStream(bytes));
            }
            var file = await s.GetFileAsync("Test/Test.txt");
            Assert.True(file.Exists);
            try
            {
                using (var stream = await file.CreateReadStreamAsync())
                {
                    bytes = stream.ReadBytesToEnd();
                    string c = Encoding.UTF8.GetString(bytes);
                    Assert.Equal(content, c);
                }
            }
            finally
            {
                await s.DeleteFileAsync("Test/Test.txt");
            }
        }

        [Fact]
        private async Task DeleteFileAsyncTest()
        {
            string filePath = $"{Guid.NewGuid().ToString("N")}.txt";
            var s = this.CreateFileStorage();

            var file = await s.GetFileAsync(filePath);
            Assert.False(file.Exists);


            await CreateNewFile(filePath, s);

            var newFile = await s.GetFileAsync(filePath);
            Assert.True(file.Exists);
            await s.DeleteFileAsync(filePath);

            var newFile2 = await s.GetFileAsync(filePath);
            Assert.False(file.Exists);
        }

        [Fact]
        private async Task ListFileAsyncTest()
        {
            string directory = Guid.NewGuid().ToString("N");
            string directory2 = $"{directory}/{Guid.NewGuid().ToString("N")}";
            string filePath1 = $"{directory}/{Guid.NewGuid().ToString("N")}.txt";
            string filePath2 = $"{directory2}/{Guid.NewGuid().ToString("N")}.txt";
            var s = this.CreateFileStorage();

            await CreateNewFile(filePath1, s);
            await CreateNewFile(filePath2, s);

            try
            {
                var files = await s.GetFilesAsync(directory, SearchOption.AllDirectories);
                Assert.Equal(2, files.Count());

                var files2 = await s.GetFilesAsync(directory, SearchOption.TopDirectoryOnly);
                Assert.Single(files2);
            }
            finally
            {
                await s.DeleteFileAsync(filePath1);
                await s.DeleteFileAsync(filePath2);
            }
        }

        [Fact]
        private async Task CreationTest()
        {
            string directory = Guid.NewGuid().ToString("N");
            string filePath1 = $"{directory}/{Guid.NewGuid().ToString("N")}.txt";
            var s = this.CreateFileStorage();

            var f = await s.GetFileAsync(filePath1);
            Assert.False(f.Exists);

            try
            {
                using (var creation = s.CreateFile(filePath1))
                {
                    using (var stream = await creation.OpenWriteStreamAsync())
                    {
                        string content = "Hellow!";
                        var bytes = Encoding.UTF8.GetBytes(content);
                        stream.Write(bytes, 0, bytes.Length);
                    }

                    await creation.SaveChangesAsync();
                }

                var f2 = await s.GetFileAsync(filePath1);
                Assert.True(f2.Exists);
            }
            finally
            {
                await s.DeleteFileAsync(filePath1);
            }
        }

        private static async Task CreateNewFile(string filePath1, PhysicalFileStorage s)
        {
            string content = "Hellow!";
            var bytes = Encoding.UTF8.GetBytes(content);
            using (MemoryStream ms = new MemoryStream())
            {
                await s.CreateFileAsync(filePath1, new MemoryStream(bytes));
            }
        }
    }
}
