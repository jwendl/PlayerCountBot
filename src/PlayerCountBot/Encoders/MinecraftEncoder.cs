using PlayerCountBot.Clients.Minecraft.Models;
using System.Text;

namespace PlayerCountBot.Encoders
{
    public class MinecraftEncoder
    {
        public const int HeaderLength = 10; // Does not include 4-byte message length.

        public static byte[] EncodeMessage(MinecraftMessage msg)
        {
            List<byte> bytes = new();

            bytes.AddRange(BitConverter.GetBytes(msg.Length));
            bytes.AddRange(BitConverter.GetBytes(msg.ID));
            bytes.AddRange(BitConverter.GetBytes((int)msg.Type));
            bytes.AddRange(Encoding.UTF8.GetBytes(msg.Body));
            bytes.AddRange(new byte[] { 0, 0 });

            return bytes.ToArray();
        }

        public static MinecraftMessage DecodeMessage(byte[] bytes)
        {
            int len = BitConverter.ToInt32(bytes, 0);
            int id = BitConverter.ToInt32(bytes, 4);
            int type = BitConverter.ToInt32(bytes, 8);

            int bodyLen = bytes.Length - (HeaderLength + 4);
            if (bodyLen > 0)
            {
                byte[] bodyBytes = new byte[bodyLen];
                Array.Copy(bytes, 12, bodyBytes, 0, bodyLen);
                Array.Resize(ref bodyBytes, bodyLen);
                return new MinecraftMessage(len, id, (MessageType)type, Encoding.UTF8.GetString(bodyBytes));
            }
            else { return new MinecraftMessage(len, id, (MessageType)type, ""); }
        }
    }
}
