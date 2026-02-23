using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace UefiResourcePackEditor.Core.Models;

public sealed class ResourcePackage: ICloneable
{
    public byte[] Header { get; set; } = new byte[32];

    public List<ResourceBlock> Blocks { get; set; } = [];

    public object Clone()
    {
        return new ResourcePackage
        {
            Header = (byte[]) Header.Clone(),
            Blocks = [.. Blocks.Select(b => (ResourceBlock) b.Clone())]
        };
    }

    public static async Task<ResourcePackage> ParseFromAsync(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanRead)
        {
            throw new ArgumentException("Stream is not readable.", nameof(stream));
        }

        ResourcePackage package = new();

        int read = await stream.ReadAsync(package.Header, 0, 0x20);
        if (read != 0x20)
        {
            throw new EndOfStreamException($"Expected to read 32 bytes for header, but only read {read} bytes.");
        }

        while (true)
        {
            ResourceBlock? block = await ResourceBlock.ParseFromAsync(stream);
            if (block is null)
            {
                break;
            }

            package.Blocks.Add(block);
        }

        return package;
    }

    public async Task SerializeToAsync(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanWrite)
        {
            throw new ArgumentException("Stream is not writable.", nameof(stream));
        }

        await stream.WriteAsync(Header, 0, Header.Length);

        foreach (ResourceBlock block in Blocks)
        {
            await block.SerializeToAsync(stream);
        }
    }
}
