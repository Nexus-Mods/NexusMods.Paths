using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NexusMods.Paths.Utilities;

/// <summary>
///     Contains all known common file extensions available for easy access.
/// </summary>
[PublicAPI]
public static class KnownExtensions
{
    // NOTE:
    // All items within each region are sorted alphabetically.
    // All regions are sorted alphabetically.
    // - Sewer

    #region Archive

    /// <summary>.7z - Archive, 7-Zip.</summary>
    public static readonly Extension _7z = new(".7z");

    /// <summary>.7zip - Archive, 7-Zip. Uncommon/rare.</summary>
    public static readonly Extension _7zip = new(".7zip");

    /// <summary>.archive - Archive. CyberPunk, Mass Effect, etc..</summary>
    public static readonly Extension Archive = new(".archive");

    /// <summary>
    ///     .afs - Archive, CRI Middleware. Common in Japanese games from 1996 till mid 2010s. 9/10 times, stores audio,
    ///     but can store data.
    /// </summary>
    public static readonly Extension Afs = new(".afs");

    /// <summary>
    ///     .awb - Archive, CRI Middleware. (a.k.a. AFS2) Common in Japanese games from 2008 onwards. Usually stores
    ///     audio.
    /// </summary>
    public static readonly Extension Awb = new(".awb");

    /// <summary>
    ///     .bank - Archive (of Audio), 'Bank'. Not a specific format, but a term, seen in a bunch of games. Usually contains multiple voice clips.
    /// </summary>
    public static readonly Extension Bank = new(".bank");

    /// <summary>.ba2 - Archive. Bethesda games.</summary>
    public static readonly Extension Ba2 = new(".ba2");

    /// <summary>
    ///     .bdt - Archive, 'Binder ??. Archive format found in Souls games.
    /// </summary>
    public static readonly Extension Bdt = new(".bdt");

    /// <summary>
    ///     .bfs - Archive, 'BugBear File System'. Common in high profile racing games produced by BugBear from the 2000s and 2010s.
    /// </summary>
    public static readonly Extension Bfs = new(".bfs");

    /// <summary>
    ///     .bht - Archive, 'Binder ??. Archive format found in Souls games.
    /// </summary>
    public static readonly Extension Bht = new(".bht");

    /// <summary>
    ///     .bnd - Archive, 'Binder'. From Software Archive format found in Souls games.
    /// </summary>
    public static readonly Extension Bnd = new(".bnd");

    /// <summary>
    ///     .bnk - Archive (of Audio), 'Bank'. Not a specific format, but a term, seen in a bunch of games. Usually contains multiple voice clips.
    /// </summary>
    public static readonly Extension Bnk = new(".bnk");

    /// <summary>.bsa - Archive. Bethesda games.</summary>
    public static readonly Extension Bsa = new(".bsa");

    /// <summary>.bundle - Archive, Unity Engine. As shipped in games. a.k.a. AssetBundle</summary>
    public static readonly Extension Bundle = new(".bundle");

    /// <summary>
    ///     .csb - Archive. CRI Middleware. Archive of Audio Files. Common in Japanese games from 2008 onwards.
    /// </summary>
    public static readonly Extension Csb = new(".csb");

    /// <summary>.cpk - Archive. CRI Middleware. Common in Japanese games from 2008 onwards.</summary>
    public static readonly Extension Cpk = new(".cpk");

    /// <summary>.fsb - Archive. FMOD Sound Bank. Common Middleware.</summary>
    public static readonly Extension Fsb = new(".fsb");

    /// <summary>.hkx - Archive. Havok Archive Format. Souls Games, Late 2000s Sonic Titles, etc.</summary>
    public static readonly Extension Hkx = new(".hkx");

    /// <summary>.nx - Archive, Nexus Mods</summary>
    public static readonly Extension Nx = new(".nx");

    /// <summary>.oiv - Archive, OpenIV. Common in GTA IV/V</summary>
    public static readonly Extension Oiv = new(".oiv");

    /// <summary>.pak - Archive, Various Sources. These days, commonly 'Unreal Engine' games.</summary>
    public static readonly Extension Pak = new(".pak");

    /// <summary>.rar - Archive, RARLab/WinRAR.</summary>
    public static readonly Extension Rar = new(".rar");

    /// <summary>.txd - Archive, (RenderWare) Texture Dictionary. Contains packed textures.</summary>
    public static readonly Extension Txd = new(".txd");

    /// <summary>.tpf - Archive, (of Images). From Software, Souls Games. Contains packed textures.</summary>
    public static readonly Extension Tpf = new(".tpf");

    /// <summary>.uasset - Archive, Unreal Engine..</summary>
    public static readonly Extension Uasset = new(".uasset");

    /// <summary>.unitypackage - Archive, Unity. Used mainly by devs.</summary>
    public static readonly Extension Unitypackage = new(".unitypackage");

    /// <summary>.vpk - Archive, Valve Software's Archive Format.</summary>
    public static readonly Extension Vpk = new(".vpk");

    /// <summary>.wad - Archive, Where's all the Data !? Used in Doom and its 1 million derivatives.</summary>
    public static readonly Extension Wad = new(".wad");

    /// <summary>.xnb - Archive, XNA Binary. Archive format for Microsoft XNA games.</summary>
    public static readonly Extension Xnb = new(".xnb");

    /// <summary>.zip - Archive, PKZip.</summary>
    public static readonly Extension Zip = new(".zip");

    #endregion

    #region Audio

    /// <summary>.m4a - Audio, Advanced Audio Coding. MP3's successor.</summary>
    public static readonly Extension Aac = new(".aac");

    /// <summary>.adx - Audio, CRI Middleware. Used for music. Common in Japanese games from 1996-2012.</summary>
    public static readonly Extension Adx = new(".adx");

    /// <summary>
    ///     .aix - Audio, CRI Middleware. A container of <see cref="Adx" /> files. Used when surround sound or dynamic
    ///     language change is needed.
    /// </summary>
    public static readonly Extension Aix = new(".aix");

    /// <summary>
    ///     .ahx - Audio, CRI Middleware. Highly compressed audio format typically used for speech. Common in Japanese
    ///     games 1996-2012.
    /// </summary>
    public static readonly Extension Ahx = new(".ahx");

    /// <summary>.fmod - Audio, FMOD Middleware.</summary>
    public static readonly Extension Fmod = new(".fmod");

    /// <summary>.flac - Audio, Free Lossless Audio Codec. Sometimes used for music.</summary>
    public static readonly Extension Flac = new(".flac");

    /// <summary>.hca - Audio, CRI Middleware. a.k.a. <see cref="Adx" />2 Common in Japanese games from 2008 onwards.</summary>
    public static readonly Extension Hca = new(".hca");

    /// <summary>.mp3 - Audio, The world's most known music format.</summary>
    public static readonly Extension Mp3 = new(".mp3");

    /// <summary>.m4a - Audio, MPEG-4 Audio. Usually <see cref="Aac" /> but can also be ALAC.</summary>
    // ReSharper disable once InconsistentNaming
    public static readonly Extension M4a = new(".m4a");

    /// <summary>.ogg - Audio, OGG Vorbis. Free codec, common in some games.</summary>
    public static readonly Extension Ogg = new(".ogg");

    /// <summary>.wav - Audio, WAVE. Uncompressed audio.</summary>
    public static readonly Extension Wav = new(".wav");

    /// <summary>.xma - Audio, Microsoft XMA. Seen in some Xbox (360) Arcade games that also made it to PC.</summary>
    public static readonly Extension Xma = new(".xma");

    /// <summary>.xwb - Audio, XACT Wave Bank. From DirectX SDK. Stores multiple <see cref="Wav" /> files.</summary>
    public static readonly Extension Xwb = new(".xwb");

    /// <summary>.xwm - Audio, Bethesda Titles.</summary>
    public static readonly Extension Xwm = new(".xwm");

    #endregion

    #region Binary (Generic)

    /// <summary>.tmp - Binary, Temporary file.</summary>
    public static readonly Extension Tmp = new(".tmp");

    /// <summary>.xsb - Audio, XACT Sound Bank. From DirectX SDK. Contains metadata for <see cref="Xwb" /> files.</summary>
    public static readonly Extension Xsb = new(".xsb");

    #endregion

    #region Database

    /// <summary>.sqlite - Database, generic extension common in databases.</summary>
    public static readonly Extension Db = new(".db");

    /// <summary>.sqlite - Database, Database. Used by SQLite.</summary>
    public static readonly Extension Sqlite = new(".sqlite");

    /// <summary>.esp - Database, Bethesda, Elder Scrolls Plugin.</summary>
    public static readonly Extension Esp = new(".esp");

    /// <summary>.esp - Database, Bethesda, Elder Scrolls Master.</summary>
    public static readonly Extension Esm = new(".esm");

    #endregion

    #region Executable (No Scripts)

    /// <summary>.app - Executable, Executable format on macOS.</summary>
    public static readonly Extension App = new(".app");

    /// <summary>.elf - Executable, Executable Linkable Format, common in Unix style land.</summary>
    public static readonly Extension Elf = new(".elf");

    /// <summary>.exe - Executable, Portable Executable, Microsoft Windows.</summary>
    public static readonly Extension Exe = new(".exe");

    /// <summary>.jar - Executable, Java Archive.</summary>
    public static readonly Extension Jar = new(".jar");

    #endregion

    #region Image

    /// <summary>.bmp - Image, Bitmap. Sometimes used for textures in old games.</summary>
    public static readonly Extension Bmp = new(".bmp");

    /// <summary>.dds - Image, Direct Draw Surface. Commonly used for textures.</summary>
    public static readonly Extension Dds = new(".dds");

    /// <summary>.gif - Image. Your favourite format for animated images. Surprising, but you may find it in some 90s games.</summary>
    public static readonly Extension Gif = new(".gif");

    /// <summary>.jpg - Image, JPEG.</summary>
    public static readonly Extension Jpg = new(".jpg");

    /// <summary>.jxl - Image, JPEG XL.</summary>
    public static readonly Extension Jxl = new(".jxl");

    /// <summary>.png - Image, Portable Network Graphics.</summary>
    public static readonly Extension Png = new(".png");

    /// <summary>.pvr - Image, PVR. (Also Sega Ninja & NN based games have a 'pvr' texture format)</summary>
    public static readonly Extension Pvr = new(".pvr");

    /// <summary>.tga - Image, Targa. Used by some older games.</summary>
    public static readonly Extension Tga = new(".tga");

    /// <summary>.tga - Image, Valve's Texture Format.</summary>
    public static readonly Extension Vtf = new(".vtf");

    #endregion

    #region Library (Dynamic Linked Code)

    /// <summary>
    ///     .asi - <see cref="Dll" />. It's a renamed DLL.
    ///     Some GTA games loaded all of these on boot and it became tradition.
    /// </summary>
    public static readonly Extension Asi = new(".asi");

    /// <summary>.dds - Library, Dynamic Link Library. Windows.</summary>
    public static readonly Extension Dll = new(".dll");

    /// <summary>.dylib - Library, Dynamic Library. macOS.</summary>
    public static readonly Extension Dylib = new(".dylib");

    /// <summary>.so - Library, Shared Object. Linux & Unix-like.</summary>
    public static readonly Extension So = new(".so");

    #endregion

    #region Model (3D)

    /// <summary>.bsp - 3D Model. Blender Project</summary>
    public static readonly Extension Blend = new(".blend");

    /// <summary>.bsp - 3D Model. Commonly used in Valve's Source Titles, Quake and RenderWare 3</summary>
    public static readonly Extension Bsp = new(".bsp");

    /// <summary>
    ///     .bsp - 3D Model. COLLADA. XML based format for 3D Models. Sometimes used in more indie games, though not
    ///     commonly.
    /// </summary>
    public static readonly Extension Dae = new(".dae");

    /// <summary>.fbx - 3D Model, Filmbox. Proprietary, used commonly for transferring models between software.</summary>
    public static readonly Extension Fbx = new(".fbx");

    /// <summary>.fbx - 3D Model, GL Transmission Format. Binary royalty free format for 3D models.</summary>
    public static readonly Extension Gltf = new(".gltf");

    /// <summary>.fbx - 3D Model, See <see cref="Gltf" />.</summary>
    public static readonly Extension Glb = new(".glb");

    /// <summary>.mdl - 3D Model, Used in Valve Titles.</summary>
    public static readonly Extension Mdl = new(".mdl");

    /// <summary>.nif - 3D Model, NetImmerse. Commonly seen in Bethesda titles.</summary>
    public static readonly Extension Nif = new(".nif");

    /// <summary>.obj - 3D Model, WaveFront OBJ. Simple 3D format. Commonly used in transferring models.</summary>
    public static readonly Extension Obj = new(".obj");

    /// <summary>.x - 3D Model. An old DirectX Model Format</summary>
    public static readonly Extension X = new(".x");

    /// <summary>.xno - 3D Model. Sega NN Graphics Library</summary>
    public static readonly Extension Xno = new(".xno");

    /// <summary>.xno - 3D Model. Sega NN Graphics Library</summary>
    public static readonly Extension Zno = new(".zno");

    #endregion

    #region Script (Interpreted or JIT'ted Code as Human Readable Text)

    /// <summary>.cs - Script, Batch Script.</summary>
    public static readonly Extension Bat = new(".bat");

    /// <summary>.cs - Script, CSharp Source.</summary>
    public static readonly Extension Cs = new(".cs");

    /// <summary>.fx - Script, DirectX Effects System Shaders.</summary>
    public static readonly Extension Fx = new(".fx");

    /// <summary>.glsl - Script, OpenGL Shading Language. OpenGL.</summary>
    public static readonly Extension Glsl = new(".glsl");

    /// <summary>.hks - Script, <see cref="Lua"/>, in Souls games.</summary>
    public static readonly Extension Hks = new(".hks");

    /// <summary>.hlsl - Script, High Level Shading Language. DirectX.</summary>
    public static readonly Extension Hlsl = new(".hlsl");

    /// <summary>.lua - Script, LUA.</summary>
    public static readonly Extension Lua = new(".lua");

    /// <summary>.py - Script, Python.</summary>
    public static readonly Extension Py = new(".py");

    /// <summary>.py - Script, Shell Script.</summary>
    public static readonly Extension Sh = new(".sh");

    /// <summary>.ws - Script, Witcher Script. Seen in certain CD Project Red games.</summary>
    public static readonly Extension Ws = new(".ws");

    #endregion

    #region Text (Data, Config, Text Files)

    /// <summary>.7z - Text, Config File. Arbitrary format.</summary>
    public static readonly Extension Cfg = new(".cfg");

    /// <summary>.json - Text, JavaScript Object Notation. Common data and/or config format.</summary>
    public static readonly Extension Json = new(".json");

    /// <summary>.7z - Text, Config File. Arbitrary format.</summary>
    public static readonly Extension Ini = new(".ini");

    /// <summary>.log - Text, Used for log files by some programs.</summary>
    public static readonly Extension Log = new(".log");

    /// <summary>.md - Text, Markdown.</summary>
    public static readonly Extension Md = new(".md");

    /// <summary>.pdf - Text, Portable Document Format.</summary>
    public static readonly Extension Pdf = new(".pdf");

    /// <summary>.preset - Text, CyberPunk specific pre-configured settings format.</summary>
    public static readonly Extension Preset = new(".preset");

    /// <summary>.txt - Text, Plain Text File.</summary>
    public static readonly Extension Txt = new(".txt");

    /// <summary>.toml - Text, Tom's Markup Language. Commonly used for config files.</summary>
    public static readonly Extension Toml = new(".toml");

    /// <summary>.xml - Text, XML. Common data and/or config format.</summary>
    public static readonly Extension Xml = new(".xml");

    /// <summary>.yml - Text, YAML. Common data and/or config format.</summary>
    public static readonly Extension Yml = new(".yml");

    /// <summary>.yml - Text, YAML. <see cref="Yml" />.</summary>
    public static readonly Extension Yaml = new(".yaml");

    /// <summary>.vmf - Text, Valve Map Format. Stores info about asset placement.</summary>
    public static readonly Extension Vmf = new(".vmf");

    #endregion

    #region Video

    /// <summary>.bik - Video, Bink Video. Common in 2000s games.</summary>
    public static readonly Extension Bik = new(".bik");

    /// <summary>.bk2 - Video, Bink Video. Successor of <see cref="Bik"/>. Common in 2010s games.</summary>
    public static readonly Extension Bk2 = new(".bk2");

    /// <summary>.mp4 - Video, MPEG-4 Part 14.</summary>
    public static readonly Extension Mp4 = new(".mp4");

    /// <summary>
    ///     .sfd - Video, SofDec. CRI MiddleWare's container usually consisting
    ///     of MPEG1 video and <see cref="Adx" /> audio.
    /// </summary>
    public static readonly Extension Sfd = new(".sfd");

    /// <summary>
    ///     .usm - Video, USM. CRI MiddleWare's successor to SofDec (<see cref="Sfd" />). a.k.a. SofDec2.
    ///     Commonly uses H264 for video, and <see cref="Adx" /> or <see cref="Hca" /> for Audio Common in 2015 onwards.
    /// </summary>
    public static readonly Extension Usm = new(".usm");

    /// <summary>
    ///     .wmv Video, Windows Media Video. Sometimes seen in games which don't use any specialized middleware.
    /// </summary>
    public static readonly Extension Wmv = new(".wmv");

    #endregion

    #region Helper Methods

    /// <summary>
    /// Gets the category of a given extension.
    /// </summary>
    /// <param name="extension">The extension to get the category for.</param>
    /// <returns>The <see cref="ExtensionCategory"/> that the extension belongs to.</returns>
    public static ExtensionCategory GetCategory(this Extension extension)
    {
        return ExtensionToCategoryMap.GetValueOrDefault(extension, ExtensionCategory.Unknown);
    }

    #endregion

    private static readonly Dictionary<Extension, ExtensionCategory> ExtensionToCategoryMap = new()
    {
        // Note (Sewer): It'd be nice to source generate this.

        // Archives
        { _7z, ExtensionCategory.Archive },
        { _7zip, ExtensionCategory.Archive },
        { Archive, ExtensionCategory.Archive },
        { Afs, ExtensionCategory.Archive },
        { Awb, ExtensionCategory.Archive },
        { Ba2, ExtensionCategory.Archive },
        { Bdt, ExtensionCategory.Archive },
        { Bht, ExtensionCategory.Archive },
        { Bfs, ExtensionCategory.Archive },
        { Bank, ExtensionCategory.Audio }, // Based on fact it stores audio.
        { Bnk, ExtensionCategory.Audio }, // Based on fact it stores audio.
        { Bsa, ExtensionCategory.Archive },
        { Bundle, ExtensionCategory.Archive },
        { Csb, ExtensionCategory.Audio }, // Based on fact it stores audio.
        { Cpk, ExtensionCategory.Archive },
        { Fsb, ExtensionCategory.Audio }, // Based on fact it stores audio.
        { Hkx, ExtensionCategory.Archive },
        { Nx, ExtensionCategory.Archive },
        { Oiv, ExtensionCategory.Archive },
        { Pak, ExtensionCategory.Archive },
        { Rar, ExtensionCategory.Archive },
        { Txd, ExtensionCategory.Image }, // Based on fact it's texture container.
        { Tpf, ExtensionCategory.Image }, // Based on fact it's texture container.
        { Uasset, ExtensionCategory.Archive },
        { Unitypackage, ExtensionCategory.Archive },
        { Vpk, ExtensionCategory.Archive },
        { Wad, ExtensionCategory.Archive },
        { Xnb, ExtensionCategory.Archive },
        { Zip, ExtensionCategory.Archive },
        // Audio
        { Aac, ExtensionCategory.Audio },
        { Adx, ExtensionCategory.Audio },
        { Aix, ExtensionCategory.Audio },
        { Ahx, ExtensionCategory.Audio },
        { Fmod, ExtensionCategory.Audio },
        { Flac, ExtensionCategory.Audio },
        { Hca, ExtensionCategory.Audio },
        { Mp3, ExtensionCategory.Audio },
        { M4a, ExtensionCategory.Audio },
        { Ogg, ExtensionCategory.Audio },
        { Wav, ExtensionCategory.Audio },
        { Xma, ExtensionCategory.Audio },
        { Xwb, ExtensionCategory.Audio },
        { Xwm, ExtensionCategory.Audio },
        // Binary
        { Tmp, ExtensionCategory.Binary },
        { Xsb, ExtensionCategory.Binary },
        // Database
        { Sqlite, ExtensionCategory.Database },
        { Esp, ExtensionCategory.Database },
        { Esm, ExtensionCategory.Database },
        // Executable
        { App, ExtensionCategory.Executable },
        { Elf, ExtensionCategory.Executable },
        { Exe, ExtensionCategory.Executable },
        { Jar, ExtensionCategory.Executable },
        // Image
        { Bmp, ExtensionCategory.Image },
        { Dds, ExtensionCategory.Image },
        { Gif, ExtensionCategory.Image },
        { Jpg, ExtensionCategory.Image },
        { Jxl, ExtensionCategory.Image },
        { Png, ExtensionCategory.Image },
        { Tga, ExtensionCategory.Image },
        { Vtf, ExtensionCategory.Image },
        // Library
        { Asi, ExtensionCategory.Library },
        { Dll, ExtensionCategory.Library },
        { Dylib, ExtensionCategory.Library },
        { So, ExtensionCategory.Library },
        // 3D Model
        { Blend, ExtensionCategory.Model },
        { Bsp, ExtensionCategory.Model },
        { Dae, ExtensionCategory.Model },
        { Fbx, ExtensionCategory.Model },
        { Gltf, ExtensionCategory.Model },
        { Glb, ExtensionCategory.Model },
        { Mdl, ExtensionCategory.Model },
        { Nif, ExtensionCategory.Model },
        { Obj, ExtensionCategory.Model },
        { X, ExtensionCategory.Model },
        { Xno, ExtensionCategory.Model },
        { Zno, ExtensionCategory.Model },
        // Script
        { Bat, ExtensionCategory.Script },
        { Cs, ExtensionCategory.Script },
        { Fx, ExtensionCategory.Script },
        { Glsl, ExtensionCategory.Script },
        { Hks, ExtensionCategory.Script },
        { Hlsl, ExtensionCategory.Script },
        { Lua, ExtensionCategory.Script },
        { Py, ExtensionCategory.Script },
        { Sh, ExtensionCategory.Script },
        { Ws, ExtensionCategory.Script },
        // Text
        { Cfg, ExtensionCategory.Text },
        { Json, ExtensionCategory.Text },
        { Ini, ExtensionCategory.Text },
        { Log, ExtensionCategory.Text },
        { Md, ExtensionCategory.Text },
        { Pdf, ExtensionCategory.Text },
        { Preset, ExtensionCategory.Text },
        { Txt, ExtensionCategory.Text },
        { Toml, ExtensionCategory.Text },
        { Xml, ExtensionCategory.Text },
        { Yml, ExtensionCategory.Text },
        { Yaml, ExtensionCategory.Text },
        { Vmf, ExtensionCategory.Text },
        // Video
        { Bik, ExtensionCategory.Video },
        { Bk2, ExtensionCategory.Video },
        { Mp4, ExtensionCategory.Video },
        { Sfd, ExtensionCategory.Video },
        { Usm, ExtensionCategory.Video },
        { Wmv, ExtensionCategory.Video },
    };
}

/// <summary>
///     Provides categories for each common extension.
/// </summary>
[PublicAPI]
public enum ExtensionCategory
{
    /// <summary>
    /// Represents archive and compressed file formats.
    /// </summary>
    Archive,

    /// <summary>
    /// Represents audio and sound file formats.
    /// </summary>
    Audio,

    /// <summary>
    /// Represents binary.
    /// </summary>
    Binary,

    /// <summary>
    /// Represents database and data storage formats.
    /// </summary>
    Database,

    /// <summary>
    /// Represents executable and application file formats.
    /// </summary>
    Executable,

    /// <summary>
    /// Represents image and graphic file formats.
    /// </summary>
    Image,

    /// <summary>
    /// Represents dynamic library and code library formats.
    /// </summary>
    Library,

    /// <summary>
    /// Represents 3D model and related file formats.
    /// </summary>
    Model,

    /// <summary>
    /// Represents script and source code file formats.
    /// </summary>
    Script,

    /// <summary>
    /// Represents text, configuration, and documentation file formats.
    /// </summary>
    Text,

    /// <summary>
    /// Represents video, movie, and cutscene file formats.
    /// </summary>
    Video,

    /// <summary>
    /// Unknown file format.
    /// </summary>
    Unknown,
}
