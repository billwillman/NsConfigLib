using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Utils;

namespace NsLib.Config {

    // 配置文件头
    internal struct ConfigFileHeader {
        public ushort flag {
            get;
            private set;
        }

        // 版本号
        public ushort version {
            get;
            private set;
        }

        // indexOffset
        public long indexOffset {
            get;
            set;
        }

        public bool IsVaild {
            get {
                return flag == _Flag && indexOffset > 0;
            }
        }

        public uint Count {
            get;
            private set;
        }

        public ConfigFileHeader(uint cnt, long indexOffset) {
            this.Count = cnt;
            this.flag = _Flag;
            this.version = _CurrentVersion;
            this.indexOffset = indexOffset;
        }

        public bool LoadFromStream(Stream stream) {
            if (stream == null)
                return false;
            try {
                flag = (ushort)FilePathMgr.Instance.ReadShort(stream);

                version = (ushort)FilePathMgr.Instance.ReadShort(stream);
                Count = (uint)FilePathMgr.Instance.ReadInt(stream);
                indexOffset = FilePathMgr.Instance.ReadLong(stream);
            } catch {
                return false;
            }

            return true;
        }

        public void SeekFileToHeader(Stream stream) {
            if (stream == null)
                return;
            stream.Seek(0, SeekOrigin.Begin);
        }

        public bool SaveToStream(Stream stream) {
            if (stream == null)
                return false;
            FilePathMgr.Instance.WriteShort(stream, (short)flag);
            FilePathMgr.Instance.WriteShort(stream, (short)version);
            FilePathMgr.Instance.WriteInt(stream, (int)Count);
            FilePathMgr.Instance.WriteLong(stream, indexOffset);
            return true;
        }

        private static readonly ushort _CurrentVersion = 1;
        private static readonly ushort _Flag = 0xFEFE;
    }


}