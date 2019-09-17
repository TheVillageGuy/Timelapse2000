using System;
using System.Collections;
using System.IO;

namespace RimworldRendererMod.RemoteRenderer.IO
{
    public static class FileIO
    {
        public static string[] GetAllFilesSorted(string folder, params string[] endings)
        {
            if (!Directory.Exists(folder))
                return new string[0];

            var stuff = Directory.GetFiles(folder);

            (string path, DateTime written)[][] files = new(string path, DateTime written)[endings.Length][];
            int length = 0;
            for (int i = 0; i < endings.Length; i++)
            {
                string[] temp = Directory.GetFiles(folder, endings[i], SearchOption.TopDirectoryOnly);
                (string path, DateTime written)[] arr = new(string path, DateTime written)[temp.Length];

                for (int j = 0; j < temp.Length; j++)
                {
                    (string path, DateTime written) data;
                    data.path = temp[j];
                    data.written = new FileInfo(data.path).LastWriteTimeUtc;
                    arr[j] = data;
                }

                files[i] = arr;

                length += files[i].Length;
            }

            (string path, DateTime written)[] all = new(string path, DateTime written)[length];
            int currentPos = 0;
            for (int i = 0; i < files.Length; i++)
            {
                Array.Copy(files[i], 0, all, currentPos, files[i].Length);
                currentPos += files[i].Length;
            }

            Array.Sort(all, new FileComparer());

            foreach (var pair in all)
            {
                Console.WriteLine($"{new FileInfo(pair.path).Name}: {pair.written.ToLongTimeString()}");
            }

            string[] normal = new string[all.Length];
            for (int i = 0; i < all.Length; i++)
            {
                normal[i] = all[i].path;
            }

            return normal;
        }

        private class FileComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                (string path, DateTime written) a = ((string path, DateTime written))x;
                (string path, DateTime written) b = ((string path, DateTime written))y;

                return a.written.CompareTo(b.written);
            }
        }
    }
}
