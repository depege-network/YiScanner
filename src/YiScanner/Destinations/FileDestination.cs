﻿using System.IO;
using System.Threading.Tasks;
using Wikiled.Core.Utility.Arguments;
using Wikiled.YiScanner.Client;

namespace Wikiled.YiScanner.Destinations
{
    public class FileDestination : IDestination
    {
        private readonly string destination;

        public FileDestination(string destination)
        {
            Guard.NotNullOrEmpty(() => destination, destination);
            this.destination = destination;
        }

        public bool IsDownloaded(VideoHeader header)
        {
            Guard.NotNull(() => header, header);
            var fileDestination = header.GetPath(destination);
            return File.Exists(fileDestination);
        }

        public async Task Transfer(VideoHeader header, Stream source)
        {
            Guard.NotNull(() => header, header);
            Guard.NotNull(() => source, source);
            var fileDestination = header.GetPath(destination);
            using (StreamWriter write = new StreamWriter(fileDestination))
            {
                await source.CopyToAsync(write.BaseStream).ConfigureAwait(false);
            }
        }
    }
}
