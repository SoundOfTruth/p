using System.Net;
using System.Net.Sockets;

class RTPHeader
{
    public byte Version { get; set; }
    public bool Padding { get; set; }
    public bool Extension { get; set; }
    public byte CC { get; set; }
    public byte PayloadType { get; set; }
    public ushort SequenceNumber { get; set; }
    public uint Timestamp { get; set; }
    public uint SSRC { get; set; }

    public byte[] GetHeaderBytes()
    {
        byte[] header = new byte[12];
        header[0] = (byte)((Version << 6) | (Convert.ToByte(Padding) << 5) | (Convert.ToByte(Extension) << 4) | CC);
        header[1] = PayloadType;
        header[2] = (byte)(SequenceNumber >> 8);
        header[3] = (byte)(SequenceNumber & 0xFF);
        header[4] = (byte)(Timestamp >> 24);
        header[5] = (byte)((Timestamp >> 16) & 0xFF);
        header[6] = (byte)((Timestamp >> 8) & 0xFF);
        header[7] = (byte)(Timestamp & 0xFF);
        header[8] = (byte)(SSRC >> 24);
        header[9] = (byte)((SSRC >> 16) & 0xFF);
        header[10] = (byte)((SSRC >> 8) & 0xFF);
        header[11] = (byte)(SSRC & 0xFF);

        return header;
    }
}

class Program
{
    static void Main()
    {
        UdpClient udpClient = new UdpClient();
        string remoteIPAddress = "127.0.0.1";
        int remotePort = 5004;

        RTPHeader rtpHeader = new RTPHeader
        {
            Version = 2,
            Padding = false,
            Extension = false,
            CC = 0,
            PayloadType = 0,
            SequenceNumber = 12345,
            Timestamp = 987654321,
            SSRC = 123456789
        };

        byte[] buffer = new byte[255];
        byte[] data = CombineBytes(rtpHeader.GetHeaderBytes(), buffer);

        udpClient.Send(data, data.Length, remoteIPAddress, remotePort);
    }
    static byte[] CombineBytes(byte[] first, byte[] second)
    {
        byte[] result = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
        return result;
    }
}
