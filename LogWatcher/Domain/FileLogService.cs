﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Interop;
using LogWatcher.Domain.Messages;
using LogWatcher.Infrastructure;

namespace LogWatcher.Domain
{
    class FileLogService : ILogService
    {
        private FilePoller _filePoller;
        private readonly FileReader _fileReader;

        public FileLogService()
        {
            _fileReader = new FileReader();
            
            Message.Subscribe<FileChangeDetectedMessage>(async msg => await OnFileChangeDetected(msg));
        }

        private async Task OnFileChangeDetected(FileChangeDetectedMessage message)
        {
            var newLines = await _fileReader.ReadChanges(message.File);
            newLines.ToList().ForEach(line => Message.Publish(new NewLogEntryMessage { LogEntry = LogEntry.Parse(new BasicTextFormat(), message.File.Name, line) }));
        }

        public void StartProcessing(params string[] parameters)
        {
            if (!VerifyHasRequiredParameters(parameters)) return;

            var filePath = parameters[0];
            var file = new FileInfo(filePath);
            if (file.Exists)
            {
                _filePoller = new FilePoller(file);
                _filePoller.Start();
            }
        }

        private bool VerifyHasRequiredParameters(string[] parameters)
        {
            var filepath = parameters[0];
            return !String.IsNullOrEmpty(filepath) && File.Exists(filepath);
        }
    }
}
