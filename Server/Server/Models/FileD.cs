
namespace Server.Models
{
    public struct FileD
    {
        public int ID;
        public string FileName;
        public string From;
        public string To;
        public int FileSize;
        public byte[] fileBuffer;
    }
}