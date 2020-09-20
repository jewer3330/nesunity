# 8位模拟器哈 unity csharp 版本
## 第一章 加载ROM
``` csharp
/// <summary>
        /// 文件头:
        /// 0-3: string    "NES"<EOF>
        ///   4: byte 以16384(0x4000)字节作为单位的PRG-ROM大小数量
        ///   5: byte 以 8192(0x2000)字节作为单位的CHR-ROM大小数量
        ///   6: bitfield Flags 6
        ///   7: bitfield Flags 7
        ///8-15: byte 保留用, 应该为0. 其实有些在用了, 目前不管

        ///CHR-ROM - 角色只读存储器(用于图像显示, 暂且不谈)

        ///Flags 6:
        ///7       0
        ///---------
        ///NNNN FTBM

        ///N: Mapper编号低4位
        ///F: 4屏标志位. (如果该位被设置, 则忽略M标志)
        ///T: Trainer标志位.  1表示 $7000-$71FF加载 Trainer
        ///B: SRAM标志位 $6000-$7FFF拥有电池供电的SRAM.
        ///M: 镜像标志位.  0 = 水平, 1 = 垂直.

        ///Byte 7 (Flags 7):
        ///7       0
        ///---------
        ///NNNN xxPV

        ///N: Mapper编号高4位
        ///P: Playchoice 10标志位.被设置则表示为PC-10游戏
        ///V: Vs.Unisystem标志位.被设置则表示为Vs.游戏
        ///x: 未使用
        /// </summary>
        /// <param name="filename"></param>
        public Cartridge(string filename)
        {
            Raw = System.IO.File.ReadAllBytes(filename);

            int header = BitConverter.ToInt32(Raw, 0);
            if (header != 0x1A53454E) // "NES<EOF>"
                throw new FormatException("unexpected header value " + header.ToString("X"));

            PRGROMSize = Raw[4] * 0x4000; // 16kb units
            CHRROMSize = Raw[5] * 0x2000; // 8kb units
            PRGRAMSize = Raw[8] * 0x2000;

            bool hasTrainer = (Raw[6] & 0b100) > 0;
            PRGROMOffset = 16 + (hasTrainer ? 512 : 0);

            MirroringMode = (Raw[6] & 0x1) > 0 ? VRAMMirroringMode.Vertical : VRAMMirroringMode.Horizontal;
            if ((Raw[6] & 0x8) > 0) MirroringMode = VRAMMirroringMode.All;

            MapperNumber = (Raw[6] >> 4) | (Raw[7] & 0xF0);

            PRGROM = new byte[PRGROMSize];
            Array.Copy(Raw, PRGROMOffset, PRGROM, 0, PRGROMSize);

            if (CHRROMSize == 0)
                CHRROM = new byte[0x2000];
            else
            {
                CHRROM = new byte[CHRROMSize];
                Array.Copy(Raw, PRGROMOffset + PRGROMSize, CHRROM, 0, CHRROMSize);
            }
        }
```