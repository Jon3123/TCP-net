using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
namespace net
{

    public class Net
    {
        TcpListener server;
        TcpClient client;
        Byte[] readbuffer;
        int positionOfReadBuffer;
        public MemoryStream writebuffer;
        public void CreateBuffers()
        {
            ClearBuffer();
        }

        public TcpListener TCPListen(int port_number)
        {
            server = new TcpListener(IPAddress.Any, port_number);
            CreateBuffers();
            server.Start();

            return server;
        }
        public TcpClient TCPConnect(String ipaddress, int port_number)
        {
            try
            {
                client = new TcpClient();
                client.Connect(ipaddress, port_number);
                client.Client.Blocking = true;
                return client;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static TcpClient TCPAccept(TcpListener l)
        {
            if (!l.Pending())
            {
                return null;
            }
            else
            {
                return l.AcceptTcpClient();
            }
        }
        public void ClearBuffer()
        {
            writebuffer = new MemoryStream(255);
            WriteByte(0);
            WriteByte(0);
        }

        public void WriteByte(int bytevalue)
        {
            writebuffer.WriteByte((byte)bytevalue);
        }
        public void WriteShort(int shortvalue)
        {
            int val1 = shortvalue;
            int val2 = shortvalue >> 8;
            WriteByte(val1);
            WriteByte(val2);
        }
        public void WriteInt(int intvalue)
        {
            Byte[] array = BitConverter.GetBytes(intvalue);
            for (int i = 3; i > -1; i--)
            {
                WriteByte(array[i]);
            }
        }
        public void WriteString(string str)
        {
            char[] array = str.ToCharArray();
            for (var i = 0; i < array.Length; i++)
            {
                WriteByte(array[i]);
            }
            WriteByte(0);
        }
        public int ReadByte()
        {

            int val = (int)readbuffer[positionOfReadBuffer];
            if (positionOfReadBuffer + 1 < readbuffer.Length)
                positionOfReadBuffer++;
            return val;
        }

        public int ReadShort()
        {
            int val = BitConverter.ToInt16(readbuffer, positionOfReadBuffer);
            if (positionOfReadBuffer + 2 < readbuffer.Length)
                positionOfReadBuffer += 2;
            return val;
        }

        public int ReadInt()
        {
            int val = BitConverter.ToInt32(readbuffer, positionOfReadBuffer);
            if (positionOfReadBuffer + 4 < readbuffer.Length)
                positionOfReadBuffer += 4;
            return val;
        }
        public String ReadString()
        {
            String s = "";
            for (int i = positionOfReadBuffer; i < readbuffer.Length; i++)
            {
                int val = ReadByte();
                if (val == 0)
                    break;
                char character = (char)val;
                s = s + character;
            }
            return s;
        }
        public void SendMessage(Socket sock)
        {

            Byte[] array = new Byte[2];
            array[0] = (byte)writebuffer.ToArray().Length;
            array[1] = 0;
            writebuffer.Position = 0;
            writebuffer.Write(array, 0, 2);
            sock.Send(writebuffer.GetBuffer());
        }
        public int ReadMessages(NetworkStream stream)
        {
            try
            {
                byte[] array = new Byte[256];
                stream.Read(array, 0, 256);
                int size = array[0];
                readbuffer = new byte[size + 2];
                stream.Read(readbuffer, 0, size + 2);
                positionOfReadBuffer = 2;


                return size;

            }
            catch (Exception e)
            {
                return -1;
            }
        }
    }

}