using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace IEC.Target.SqlServer
{
    public class TargetFileNameController
    {
        public const string Extension = "bin";

        public int KeepDays
        {
            get;
        }

        public string TargetFolderPath
        {
            get;
        }

        public long MaxFileLength
        {
            get;
        }

        public int MaxFilesPerDay
        {
            get;
        }

        private int _currentFileIndex;
        private DateTime _currentDateTime;

        private DateTime _lastCleanupDate = DateTime.Now.Date.AddDays(-1);

        public TargetFileNameController(
            int keepDays,
            string targetFolderPath,
            long maxFileLength,
            int maxFilesPerDay = int.MaxValue
            )
        {
            if (targetFolderPath == null)
            {
                throw new ArgumentNullException(nameof(targetFolderPath));
            }

            if (keepDays < 1)
            {
                throw new ArgumentException(nameof(keepDays));
            }
            if (maxFileLength < 1)
            {
                throw new ArgumentException(nameof(targetFolderPath));
            }
            if (maxFilesPerDay < 1)
            {
                throw new ArgumentException(nameof(targetFolderPath));
            }

            KeepDays = keepDays;
            TargetFolderPath = targetFolderPath;
            MaxFileLength = maxFileLength;
            MaxFilesPerDay = maxFilesPerDay;

            if (!Directory.Exists(targetFolderPath))
            {
                Directory.CreateDirectory(targetFolderPath);
            }

            (_currentDateTime, _currentFileIndex) = DetermineCurrentFileName(maxFilesPerDay);
        }

        internal void SwitchTargetFile()
        {
            var now = DateTime.Now;
            if (now.Date != _currentDateTime.Date)
            {
                //new date!

                //generate new name
                _currentFileIndex = 0;
                _currentDateTime = now;

                var (targetFilePath, _) = BuildCurrentFile();
                if (File.Exists(targetFilePath))
                {
                    File.Delete(targetFilePath);
                }

                return;
            }

            {
                if(_currentFileIndex == MaxFilesPerDay - 1)
                {
                    throw new InvalidOperationException($"Out of file count for {now:yyyyMMdd}");
                }

                //we need to increment an index
                _currentFileIndex++;

                var (targetFilePath, _) = BuildCurrentFile();
                if (File.Exists(targetFilePath))
                {
                    File.Delete(targetFilePath);
                }

                return;
            }
        }


        internal (string filePath, string folderPath, string fileName) GetTargetFile()
        {
            var now = DateTime.Now;
            if (now.Date != _currentDateTime.Date)
            {
                SwitchTargetFile();
            }

            var (targetFilePath, _) = BuildCurrentFile();

            if (File.Exists(targetFilePath))
            {
                var length = new FileInfo(targetFilePath).Length;
                if (length >= MaxFileLength)
                {
                    SwitchTargetFile();
                }
            }

            //if switches were performed, the target file may change!

            var (resultFilePath, resultFileName) = BuildCurrentFile();

            return (resultFilePath, TargetFolderPath, resultFileName);
        }


        internal bool Cleanup(
            out DateTime border
            )
        {
            var nowDate = DateTime.Now.Date;

            if (_lastCleanupDate.Date == nowDate)
            {
                //no day change occurs, no cleanup needs
                border = nowDate;
                return false;
            }

            var result = false;

            border = nowDate.AddDays(-KeepDays);

            var toDeletes = new List<string>();
            foreach (var file in Directory.GetFiles(TargetFolderPath, $"*.{Extension}", SearchOption.TopDirectoryOnly))
            {
                var (d, _) = ParseFile(file);

                if (d < border)
                {
                    toDeletes.Add(file);
                }
            }

            foreach (var toDelete in toDeletes)
            {
                try
                {
                    File.Delete(toDelete);
                    result = true;
                }
                catch
                {
                    //force mute
                }
            }

            if (result)
            {
                _lastCleanupDate = nowDate;
            }

            return result;
        }


        
        private (DateTime fileDate, int fileIndex) DetermineCurrentFileName(
            int maxFilePerDay
            )
        {
            for (var fileIndex = 0; fileIndex < maxFilePerDay; fileIndex++)
            {
                var now = DateTime.Now.Date;

                var fileName = $"{now:yyyyMMdd}.{fileIndex}.bin";
                var filePath = Path.Combine(TargetFolderPath, fileName);

                if (File.Exists(filePath))
                {
                    var length = new FileInfo(filePath).Length;
                    if (length < MaxFileLength)
                    {
                        return (now, fileIndex);
                    }
                }
                else
                {
                    return (now, fileIndex);
                }
            }

            throw new InvalidOperationException($"Out of file count for {DateTime.Now:yyyyMMdd}");
        }

        private (DateTime fileDate, int fileIndex) ParseFile(
            string filePath
            )
        {
            if (!filePath.EndsWith(Extension))
            {
                throw new InvalidOperationException($"Unknown file");
            }

            var fileName = new FileInfo(filePath).Name;

            var dts = fileName.Substring(0, 8);

            var dt = DateTime.ParseExact(dts, "yyyyMMdd", CultureInfo.InvariantCulture);
            var index = int.Parse(
                new string(fileName.Substring(9).TakeWhile(a => a != '.').ToArray())
                );

            return (dt, index);
        }

        private (string filePath, string fileName) BuildCurrentFile()
        {
            var fileName = $"{_currentDateTime:yyyyMMdd}.{_currentFileIndex}.bin";
            var filePath = Path.Combine(TargetFolderPath, fileName);

            return (filePath, fileName);
        }
    }
}
