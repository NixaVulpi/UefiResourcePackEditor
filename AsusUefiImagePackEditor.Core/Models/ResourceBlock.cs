using System;
using System.IO;
using System.Threading.Tasks;

namespace AsusUefiImagePackEditor.Core.Models;

public sealed class ResourceBlock: ICloneable
{
    private byte[] _data = [];

    public byte[] Header { get; set; } = new byte[32];

    public byte[] Data
    {
        get => _data;
        set
        {
            _data = value;
            Size = (uint) value.Length;
        }
    }

    public ushort Id
    {
        get
        {
            unsafe
            {
                fixed (byte* ptr = &Header[0x0E])
                {
                    return *(ushort*) ptr;
                }
            }
        }
        set
        {
            unsafe
            {
                fixed (byte* ptr = &Header[0x0E])
                {
                    *(ushort*) ptr = value;
                }
            }
        }
    }

    public uint Size
    {
        get
        {
            unsafe
            {
                fixed (byte* ptr = Header)
                {
                    return *(uint*) ptr;
                }
            }
        }
        private set
        {
            unsafe
            {
                fixed (byte* ptr = Header)
                {
                    *(uint*) ptr = value;
                }
            }
        }
    }

    public object Clone()
    {
        return new ResourceBlock
        {
            Header = (byte[]) Header.Clone(),
            Data = (byte[]) Data.Clone()
        };
    }

    public static async Task<ResourceBlock?> ParseFromAsync(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (!stream.CanRead)
        {
            throw new ArgumentException("Stream is not readable.", nameof(stream));
        }

        ResourceBlock block = new();

        int read = await stream.ReadAsync(block.Header, 0, 0x20);

        if (read == 0)
        {
            return null;
        }

        if (read != 0x20)
        {
            throw new EndOfStreamException($"Expected to read 32 bytes for header, but only read {read} bytes.");
        }

        block.Data = new byte[block.Size];
        read = await stream.ReadAsync(block.Data, 0, block.Data.Length);

        if (read != block.Data.Length)
        {
            throw new EndOfStreamException($"Expected to read {block.Data.Length} bytes for data, but only read {read} bytes.");
        }

        uint alignedSize = (block.Size + 3u) & ~3u;
        uint paddingSize = alignedSize - block.Size;

        if (paddingSize > 0)
        {
            byte[] padding = new byte[paddingSize];
            read = await stream.ReadAsync(padding, 0, padding.Length);
            if (read != padding.Length)
            {
                throw new EndOfStreamException($"Expected to read {padding.Length} bytes of padding, but only read {read} bytes.");
            }
        }

        return block;
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
        await stream.WriteAsync(Data, 0, Data.Length);

        uint alignedSize = (Size + 3u) & ~3u;
        uint paddingSize = alignedSize - Size;

        if (paddingSize > 0)
        {
            byte[] padding = new byte[paddingSize];
            await stream.WriteAsync(padding, 0, padding.Length);
        }
    }
}
