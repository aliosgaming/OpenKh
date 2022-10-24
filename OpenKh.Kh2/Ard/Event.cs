using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Ard
{
    public static class Event
    {
        private const short Terminator = 0;
        private static readonly IBinaryMapping Mapping =
            MappingConfiguration.DefaultConfiguration()
                .ForType<string>(CStringReader, CStringWriter)
                .ForType<SeqVoices>(ReadVoices, WriteVoices)
                .ForType<SetCameraData>(ReadSetCameraData, WriteSetCameraData)
                .ForType<ReadAssets>(ReadLoadAssets, WriteLoadAssets)
                .ForType<SplinePoint>(ReadSplinePoint, WriteSplinePoint)
                .ForType<Light>(ReadLight, WriteLight)
                .ForType<SplineDataEnc>(ReadSplineDataEnc, WriteSplineDataEnc)
                .ForType<SeqPart>(ReadSeqPart, WriteSeqPart)
                .ForType<VibData>(ReadVibData, WriteVibData)
                .Build();

        private static readonly Dictionary<int, Type> _idType = new Dictionary<int, Type>()
        {
            [0x00] = typeof(SetProject),
            [0x01] = typeof(SetActor),
            [0x02] = typeof(SeqActorPosition),
            [0x03] = typeof(SetMap),
            [0x05] = typeof(CameraData),
            [0x06] = typeof(SeqCamera),
            [0x07] = typeof(EffectData),
            [0x08] = typeof(SetEndFrame),
            [0x09] = typeof(SeqEffect),
            [0x0A] = typeof(AttachEffect),
            [0x0B] = typeof(SeqKage),
            [0x0C] = typeof(SeqBgcol),
            [0x0D] = typeof(SeqPart),
            [0x0E] = typeof(SeqAlpha),
            [0x0F] = typeof(SetupEvent),
            [0x10] = typeof(EventStart),
            [0x11] = typeof(JumpEvent),
            [0x12] = typeof(SeqFade),
            [0x13] = typeof(SetCameraData),
            [0x14] = typeof(EntryUnk14),
            [0x15] = typeof(SeqSubtitle),
            [0x16] = typeof(BgGrupe),
            [0x17] = typeof(SeqBlur),
            [0x18] = typeof(SeqFocus),
            [0x19] = typeof(SeqTextureAnim),
            [0x1A] = typeof(SeqActorLeave),
            [0x1B] = typeof(SeqCrossFade),
            [0x1C] = typeof(SeqIk),
            [0x1D] = typeof(SplineDataEnc),
            [0x1E] = typeof(SplinePoint),
            [0x1F] = typeof(SeqSpline),
            [0x20] = typeof(SeqGameSpeed),
            [0x21] = typeof(TexFade),
            [0x22] = typeof(WideMask),
            [0x23] = typeof(SeqVoices),
            [0x24] = typeof(ReadAssets),
            [0x25] = typeof(ReadMotion),
            [0x26] = typeof(ReadAudio),
            [0x27] = typeof(SetShake),
            [0x28] = typeof(Scale),
            [0x29] = typeof(Turn),
            [0x2A] = typeof(SeData),
            [0x2B] = typeof(SeqPlayAudio),
            [0x2C] = typeof(SeqPlayAnimation),
            [0x2D] = typeof(SeqDialog),
            [0x2E] = typeof(SeqPlayBgm),
            [0x2F] = typeof(ReadBgm),
            [0x30] = typeof(SetBgm),
            [0x31] = typeof(SeqObjCamera),
            [0x32] = typeof(MusicalHeader),
            [0x33] = typeof(MusicalTarget),
            [0x34] = typeof(MusicalScene),
            [0x35] = typeof(VibData),
            [0x36] = typeof(Lookat),
            [0x37] = typeof(ShadowAlpha),
            [0x38] = typeof(ReadActor),
            [0x39] = typeof(ReadEffect),
            [0x3A] = typeof(SeqMirror),
            [0x3B] = typeof(SeqTreasure),
            [0x3C] = typeof(SeqMissionEffect),
            [0x3D] = typeof(SeqLayout),
            [0x3E] = typeof(ReadLayout),
            [0x3F] = typeof(StopEffect),
            [0x40] = typeof(CacheClear),
            [0x41] = typeof(SeqObjPause),
            [0x42] = typeof(SeqBgse),
            [0x43] = typeof(SeqGlow),
            [0x44] = typeof(RunMovie),
            [0x45] = typeof(SeqSavePoint),
            [0x46] = typeof(SeqCameraCollision),
            [0x47] = typeof(SeqPosMove),
            [0x48] = typeof(BlackFog),
            [0x49] = typeof(Fog),
            [0x4A] = typeof(PlayerOffsetCamera),
            [0x4B] = typeof(SkyOff),
            [0x4D] = typeof(SeqHideObject),
            [0x4E] = typeof(Light),
            [0x4F] = typeof(SeqMob),
            [0x50] = typeof(Countdown),
            [0x51] = typeof(Tag),
            [0x52] = typeof(WallClip),
            [0x53] = typeof(VoiceAllFadeout),
        };

        private static readonly Dictionary<Type, int> _typeId =
            _idType.ToDictionary(x => x.Value, x => x.Key);

        private class SetCameraDataHeader
        {
            [Data] public short Index { get; set; }
            [Data] public short Count { get; set; }
        }

        public class CameraValueInternal
        {
            [Data] public uint FlagData { get; set; }
            [Data] public float Value { get; set; }
            [Data] public float TangentEaseIn { get; set; }
            [Data] public float TangentEaseOut { get; set; }
        }

        private static CameraKeys GetCameraKeys(CameraValueInternal x) =>
            new CameraKeys
            {
                Interpolation = (Motion.Interpolation)(x.FlagData >> 29),
                KeyFrame = (int)((x.FlagData & 0x1FFFFFFF ^ 0x10000000) - 0x10000000),
                Value = x.Value,
                TangentEaseIn = x.TangentEaseIn,
                TangentEaseOut = x.TangentEaseOut
            };

        private static CameraValueInternal GetCameraValueInternal(CameraKeys x) =>
            new CameraValueInternal
            {
                FlagData = (uint)((((x.KeyFrame + 0x10000000) ^ 0x10000000) & 0x1FFFFFFF) |
                        ((int)x.Interpolation << 29)),
                Value = x.Value,
                TangentEaseIn = x.TangentEaseIn,
                TangentEaseOut = x.TangentEaseOut
            };

        public interface IEventEntry
        {
        }

        public class SetProject : IEventEntry // unused
        {
            [Data] public int MemSize { get; set; }

            // Always PS2_BIN_VER (2)
            [Data] public byte Version { get; set; }
            [Data] public byte ObjCameraType { get; set; }

            // Can't be greater than 32 bytes
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{nameof(SetProject)}: {Name}, Version {Version}, Camera type {ObjCameraType}, ({MemSize:X})";
        }

        public class SetActor : IEventEntry // sub_22F528
        {
            [Data] public short ObjectEntry { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{nameof(SetActor)}: ObjEntry {ObjectEntry:X}, Name {Name}, ActorID {ActorId}";
        }

        public class SeqActorPosition : IEventEntry
        {
            [Data] public short SubId { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public float PositionX { get; set; }
            [Data] public float PositionY { get; set; }
            [Data] public float PositionZ { get; set; }
            [Data] public float RotationX { get; set; }
            [Data] public float RotationY { get; set; }
            [Data] public float RotationZ { get; set; }
            [Data] public float Scale { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public short Frame { get; set; }

            public override string ToString() =>
                $"{nameof(SeqActorPosition)}: Frame {Frame}, ActorID {ActorId}, Pos({PositionX}, {PositionY}, {PositionZ}) Rot({RotationX}, {RotationY}, {RotationZ}) SubId({SubId}), {Unk02}, {Scale}";
        }

        public class SetMap : IEventEntry // unused
        {
            [Data] public short Place { get; set; }
            [Data] public string World { get; set; }

            public override string ToString() =>
                $"{nameof(SetMap)}: {World}{Place:D02}";
        }

        public class SeqCamera : IEventEntry // ignored
        {
            [Data] public short PutId { get; set; }
            [Data] public short FrameStart { get; set; }
            [Data] public short FrameEnd { get; set; }
            [Data] public short CameraId { get; set; }

            public override string ToString() =>
                $"{nameof(SeqCamera)}: CameraID {PutId}, Frame start {FrameStart}, Frame end {FrameEnd}, {CameraId}";
        }

        public class EffectData : IEventEntry
        {
            [Data] public short EffectId { get; set; }
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{nameof(EffectData)}: ";
        }

        public class CameraData : IEventEntry
        {
            [Data] public short CameraId { get; set; }
            [Data] public short Unk02 { get; set; }

            public override string ToString() =>
                $"{nameof(CameraData)}: ";
        }

        public class SetEndFrame : IEventEntry // sub_22D3B8
        {
            [Data] public short EndFrame { get; set; } // dword_35DE28
            [Data] public short Unused { get; set; }

            public override string ToString() =>
                $"{nameof(SetEndFrame)}: {EndFrame}";
        }

        public class SeqEffect : IEventEntry
        {
            [Data] public short FrameStart { get; set; }
            [Data] public short FrameLoop { get; set; }
            [Data] public short EffectId { get; set; }
            [Data] public short PaxId { get; set; }
            [Data] public short PaxEntryIndex { get; set; }
            [Data] public short EndType { get; set; }
            [Data] public short FadeFrame { get; set; }

            public override string ToString() =>
                $"{nameof(SeqEffect)}: Frame start {FrameStart}, Frame loop {FrameLoop}, Effect ID {EffectId}, PAX ID {PaxId}, PAX entry index {PaxEntryIndex}, End type {EndType}, Frame fade {FadeFrame}";
        }

        public class AttachEffect : IEventEntry
        {
            [Data] public short FrameStart { get; set; }
            [Data] public short FrameEnd { get; set; } // maybe unused?
            [Data] public short AttachEffectId { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public short BoneIndex { get; set; }
            [Data] public short PaxEntryIndex { get; set; }
            [Data] public short Type { get; set; }

            public override string ToString() =>
                $"{nameof(AttachEffect)}: Frame start {FrameStart}, Frame end {FrameEnd}, Attach effect ID {AttachEffectId}, ActorID {ActorId}, Bone index {BoneIndex}, PAX entry {PaxEntryIndex}, Type {Type}";
        }

        public class SeqKage : IEventEntry
        {
            [Data] public short FrameStart { get; set; }
            [Data] public short FrameEnd { get; set; } // maybe unused?
            [Data] public short PutId { get; set; }
            [Data] public short Flag { get; set; }

            public override string ToString() =>
                $"{nameof(SeqKage)}: ";
        }

        public class EventStart : IEventEntry // sub_22D3A8
        {
            [Data] public short FadeIn { get; set; } // dword_35DE40
            [Data] public short Unused { get; set; }

            public override string ToString() =>
                $"{nameof(EventStart)}: Fade in for {FadeIn} frames";
        }

        public class JumpEvent : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short Type { get; set; }
            [Data] public short SetNo { get; set; }
            [Data] public short Area { get; set; }
            [Data] public short Entrance { get; set; }
            [Data] public string World { get; set; }

            public override string ToString() =>
                $"{nameof(JumpEvent)}: ";
        }

        public class SeqFade : IEventEntry
        {
            public enum FadeType : short
            {
                FromBlack,
                ToBlack,
                ToWhite,
                FromBlackVariant,
                ToBlackVariant,
                FromWhite,
                ToWhiteVariant,
                FromWhiteVariant,
            }

            [Data] public short FrameIndex { get; set; }
            [Data] public short Duration { get; set; }
            [Data] public FadeType Type { get; set; }

            public override string ToString() =>
                $"{nameof(SeqFade)}: Frame {FrameIndex}, Duration {Duration}, {Type}";
        }

        public class CameraKeys
        {
            public Motion.Interpolation Interpolation { get; set; }
            public int KeyFrame { get; set; }
            public float Value { get; set; }
            public float TangentEaseIn { get; set; }
            public float TangentEaseOut { get; set; }

            public override string ToString() =>
                $"Key frame {KeyFrame}, Value {Value}, {TangentEaseIn}, {TangentEaseOut}, Interpolation {Interpolation}";
        }

        public class SetCameraData : IEventEntry
        {
            public short CameraId { get; set; }
            public List<CameraKeys> PositionX { get; set; }
            public List<CameraKeys> PositionY { get; set; }
            public List<CameraKeys> PositionZ { get; set; }
            public List<CameraKeys> LookAtX { get; set; }
            public List<CameraKeys> LookAtY { get; set; }
            public List<CameraKeys> LookAtZ { get; set; }
            public List<CameraKeys> Roll { get; set; }
            public List<CameraKeys> FieldOfView { get; set; }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{nameof(SetCameraData)}: ID {CameraId}");
                sb.AppendLine($"\tPositionX: {ToString(PositionX)}");
                sb.AppendLine($"\tPositionY: {ToString(PositionY)}");
                sb.AppendLine($"\tPositionZ: {ToString(PositionZ)}");
                sb.AppendLine($"\tLookAtX: {ToString(LookAtX)}");
                sb.AppendLine($"\tLookAtY: {ToString(LookAtY)}");
                sb.AppendLine($"\tLookAtZ: {ToString(LookAtZ)}");
                sb.AppendLine($"\tRoll: {ToString(Roll)}");
                sb.AppendLine($"\tFOV: {ToString(FieldOfView)}");
                return sb.ToString();
            }

            private string ToString(IList<CameraKeys> values)
            {
                if (values.Count == 1)
                    return values[0].ToString();
                return string.Join("\n\t\t", values);
            }

        }

        public class EntryUnk14 : IEventEntry
        {
            [Data] public short Unk00 { get; set; }

            public override string ToString() =>
                $"{nameof(EntryUnk14)}: {Unk00}";
        }

        public class SeqSubtitle : IEventEntry
        {
            [Data] public short FrameStart { get; set; }
            [Data] public short Index { get; set; }
            [Data] public short MessageId { get; set; }
            [Data] public short HideFlag { get; set; }

            public override string ToString() =>
                $"{nameof(SeqSubtitle)}: Frame {FrameStart}, MsgId {MessageId}, Index {Index}, Hide {HideFlag != 0}";
        }

        public class BgGrupe : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public byte No { get; set; }
            [Data] public byte Flag { get; set; }

            public override string ToString() =>
                $"{nameof(BgGrupe)}: {StartFrame} {No} {Flag}";
        }

        public class SeqBlur : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public byte Sw { get; set; }
            [Data] public byte Alpha { get; set; }
            [Data] public float Rot { get; set; }
            [Data] public short X { get; set; }
            [Data] public short Y { get; set; }
            [Data] public short RotFrame { get; set; }
            [Data] public short Unk0E { get; set; }

            public override string ToString() =>
                $"{nameof(SeqBlur)}: {StartFrame}, {Sw}, {Alpha}, {Rot} {X} {Y} {RotFrame} {Unk0E}";
        }

        public class SeqFocus : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public byte Sw { get; set; }
            [Data] public byte Type { get; set; }
            [Data] public int Z { get; set; }

            public override string ToString() =>
                $"{nameof(SeqFocus)}: Frame {StartFrame}, {Sw} {Type} {Z}";
        }

        public class SeqTextureAnim : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short PutId { get; set; }
            [Data] public short No { get; set; }
            [Data] public byte Flag { get; set; }
            [Data] public byte Dummy { get; set; }

            public override string ToString() =>
                $"{nameof(SeqTextureAnim)}: Frame {Frame}, {PutId}, {No}, {Flag}, {Dummy}";
        }

        public class SeqActorLeave : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short ActorId { get; set; }

            public override string ToString() =>
                $"{nameof(SeqActorLeave)}: {Frame} {ActorId}";
        }

        public class SeqCrossFade : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Duration { get; set; }

            public override string ToString() =>
                $"{nameof(SeqCrossFade)}: {Frame}, {Duration}";
        }

        public class SeqIk : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short EndFrame { get; set; }
            [Data] public short PutId { get; set; }
            [Data] public short Flag { get; set; }

            public override string ToString() =>
                $"{nameof(SeqIk)}: ";
        }

        public class SplineDataEnc : IEventEntry
        {
            public short PutId { get; set; }
            public short TransOfs { get; set; }

            public List<CameraKeys> Keys { get; set; }

            public override string ToString() =>
                $"{nameof(SplineDataEnc)}: Channel {PutId}, {TransOfs}";
        }

        public class SplinePoint : IEventEntry
        {
            public class Point
            {
                [Data] public float PositionX { get; set; }
                [Data] public float PositionY { get; set; }
                [Data] public float PositionZ { get; set; }
                [Data] public float RightX { get; set; }
                [Data] public float RightY { get; set; }
                [Data] public float RightZ { get; set; }
                [Data] public float LeftX { get; set; }
                [Data] public float LeftY { get; set; }
                [Data] public float LeftZ { get; set; }
                [Data] public float Lng { get; set; }
            }

            public short Id { get; set; }
            public short Type { get; set; }
            public float SplineLng { get; set; }

            public List<Point> Points { get; set; }

            public override string ToString() =>
                $"{nameof(SplinePoint)}: Id {Id}";
        }

        public class SeqSpline : IEventEntry
        {
            [Data] public short PutId { get; set; }
            [Data] public short StartFrame { get; set; }
            [Data] public short EndFrame { get; set; }
            [Data] public short CamOfs { get; set; }

            public override string ToString() =>
                $"{nameof(SeqSpline)}: {PutId}, {StartFrame} {EndFrame} {CamOfs}";
        }

        public class SeqGameSpeed : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short GameSpeedFrame { get; set; }
            [Data] public float Speed { get; set; }

            public override string ToString() =>
                $"{nameof(SeqGameSpeed)}: {Frame} {Speed}";
        }

        public class TexFade : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short PutId { get; set; }
            [Data] public float From { get; set; }
            [Data] public float To { get; set; }
            [Data] public float Length { get; set; }
        }

        public class WideMask : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short Flag { get; set; }

            public override string ToString() =>
                $"{nameof(WideMask)}: {StartFrame} {Flag}";
        }

        public class SeqVoices : IEventEntry
        {
            public class Voice
            {
                public short FrameStart { get; set; }
                public string Name { get; set; }

                public override string ToString() =>
                    $"Frame {FrameStart}, {Name}";
            }

            public List<Voice> Voices { get; set; }

            public override string ToString() =>
                $"{nameof(SeqVoices)}:\n\t{string.Join("\n\t", Voices)}";
        }

        public class SeqBgcol : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short EndFrame { get; set; }
            [Data] public short PutId { get; set; }
            [Data] public short Flag { get; set; }

            public override string ToString() =>
                $"{nameof(SeqBgcol)}: Frame {StartFrame}, {PutId}, {Flag}";
        }

        public class SeqPart : IEventEntry
        {
            public short StartFrame { get; set; }
            public short EndFrame { get; set; }
            public short PutId { get; set; }
            public short[] Part { get; set; }

            public override string ToString() =>
                $"{nameof(SeqPart)}: Frame {StartFrame}, ({string.Join(", ", Part)})";
        }

        public class SeqAlpha : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short EndFrame { get; set; }
            [Data] public byte StartAlpha { get; set; }
            [Data] public byte EndAlpha { get; set; }
            [Data] public short Time { get; set; }
            [Data] public short PutId { get; set; }

            public override string ToString() =>
                $"{nameof(SeqAlpha)}: Frame {StartFrame}, {StartAlpha}, {EndAlpha}, {Time}, {PutId}";
        }

        public class SetupEvent : IEventEntry // sub_22d358
        {
            public override string ToString() =>
                $"{nameof(SetupEvent)}";
        }

        public class ReadAssets : IEventEntry
        {
            public short FrameStart { get; set; }
            public short FrameEnd { get; set; }
            public short Unk06 { get; set; }
            public List<IEventEntry> Set { get; set; }

            public override string ToString() =>
                $"{nameof(ReadAssets)}: {FrameStart} {FrameEnd} {Unk06}\n\t{string.Join("\n\t", Set)}";
        }

        public class Scale : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short PutId { get; set; }
            [Data] public float Start { get; set; }
            [Data] public float End { get; set; }
            [Data] public short Frame { get; set; }
            [Data] public short Type { get; set; }
        }

        public class Turn : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short PutId { get; set; }
            [Data] public short Type { get; set; }
            [Data] public short Frame { get; set; }
            [Data] public float End { get; set; }

            public override string ToString() =>
                $"{nameof(Turn)}: {StartFrame} {PutId} {Type} {Frame} {End}";
        }

        public class SeData : IEventEntry // sub_232A48
        {
            [Data] public short PutId { get; set; }
            [Data] public short SebNumber { get; set; }
            [Data] public short WaveNumber { get; set; }

            public override string ToString() =>
                $"{nameof(SeData)}: {PutId} {SebNumber} {WaveNumber}";
        }

        public class SeqPlayAudio : IEventEntry
        {
            [Data] public short PutId { get; set; }
            [Data] public short Type { get; set; }
            [Data] public int SeNumber { get; set; }
            [Data] public short FrameStart { get; set; }
            [Data] public short Unk0A { get; set; }

            public override string ToString() =>
                $"{nameof(SeqPlayAudio)}: {PutId}, {Type}, {SeNumber}, Frame {FrameStart}, {Unk0A}";
        }

        public class SeqPlayAnimation : IEventEntry
        {
            [Data] public short FrameStart { get; set; }
            [Data] public short FrameEnd { get; set; }
            [Data] public short MotionStartFrame { get; set; }
            [Data] public short LoopStart { get; set; }
            [Data] public short LoopEnd { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public short BlendFrame { get; set; }
            [Data] public string Path { get; set; }

            public override string ToString() =>
                $"{nameof(SeqPlayAnimation)}: Frame start {FrameStart}, Frame end {FrameEnd}, {MotionStartFrame}, {LoopStart}, {LoopEnd}, ActorID {ActorId}, {BlendFrame}, {Path}";
        }

        public class SeqDialog : IEventEntry
        {
            [Data] public short FrameIndex { get; set; }
            [Data] public short PutId { get; set; }
            [Data] public short MessageId { get; set; }
            [Data] public short Type { get; set; }
            [Data] public short WinType { get; set; }

            public override string ToString() =>
                $"{nameof(SeqDialog)}: Frame index {FrameIndex}, {PutId}, MsgID {MessageId}, {Type}, {WinType}";
        }

        public class SeqPlayBgm : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short VolumeContinue { get; set; }
            [Data] public byte VolumeStart { get; set; }
            [Data] public byte VolumeEnd { get; set; }
            [Data] public byte FadeFrame { get; set; }
            [Data] public byte Bank { get; set; }

            public override string ToString() =>
                $"{nameof(SeqPlayBgm)}: Frame {Frame}, VolumeContinue {VolumeContinue}, Volume start {VolumeStart}, Volume end {VolumeEnd}, Fade frame {FadeFrame}";
        }

        public class ReadBgm : IEventEntry
        {
            [Data] public short PutId { get; set; }
            [Data] public short BgmId { get; set; }

            public override string ToString() =>
                $"{nameof(ReadBgm)}: BGM ID {BgmId}, {PutId}";
        }

        public class SetBgm : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short BankIndex { get; set; }
            [Data] public short BgmId { get; set; }

            public override string ToString() =>
                $"{nameof(SetBgm)}: BGM ID {BgmId}, {Frame} {BankIndex}";
        }

        public class SeqObjCamera : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Type { get; set; }
        }

        public class MusicalHeader : IEventEntry
        {
            [Data] public short Rhythm { get; set; }
            [Data] public short ClearScore { get; set; }
        }

        public class MusicalTarget : IEventEntry
        {
            [Data] public short AppearFrame { get; set; }
            [Data] public short Button { get; set; }
            [Data] public short CountdownNumber { get; set; }
            [Data] public short CountdownStartFrame { get; set; }
            [Data] public short Possible { get; set; }
            [Data] public short Point { get; set; }
            [Data] public short OkSceneFrame { get; set; }
            [Data] public short PutId { get; set; }
            [Data] public short Bone { get; set; }
            [Data] public short Dummy { get; set; }
        }

        public class MusicalScene : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short NgSceneFrame { get; set; }
        }

        public class VibData : IEventEntry
        {
            public short Frame { get; set; }
            public short Dummy { get; set; }
            public byte[] Data { get; set; }
        }

        public class Lookat : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short PutId { get; set; }
            [Data] public float RL { get; set; }
            [Data] public float UD { get; set; }
            [Data] public short Length { get; set; }
            [Data] public short Type { get; set; }

            public override string ToString() =>
                $"{nameof(Lookat)}: Frame {Frame}, {PutId}, {RL}, {UD}, {Length}, {Type}";
        }

        public class ShadowAlpha : IEventEntry
        {
            [Data] public short ObjectId { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public byte StartAlpha { get; set; }
            [Data] public byte EndAlpha { get; set; }
            [Data] public short Frame { get; set; }
        }

        public class ReadActor : IEventEntry
        {
            [Data] public short ObjectId { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{nameof(ReadActor)}: ObjectEntry {ObjectId:X04}, Name {Name}, ActorID {ActorId}";
        }

        public class ReadEffect : IEventEntry
        {
            [Data] public short Id { get; set; }
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{nameof(ReadEffect)}: Id {Id}, Name {Name}";
        }

        public class SeqMirror : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short PutId { get; set; }
            [Data] public short X { get; set; }
            [Data] public short Y { get; set; }
            [Data] public short Z { get; set; }
        }

        public class SeqTreasure : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Number { get; set; }
        }

        public class SeqMissionEffect : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Number { get; set; }
            [Data] public short EndType { get; set; }
        }

        public class SeqLayout : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short LayoutIndex { get; set; }
            [Data] public string LayoutName { get; set; }

            public override string ToString() =>
                $"{nameof(ReadEffect)}: Start frame {Frame}, Layout {LayoutName} Index {LayoutIndex}";
        }

        public class ReadLayout : IEventEntry
        {
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{nameof(ReadLayout)}: {Name}";
        }

        public class ReadMotion : IEventEntry
        {
            [Data] public short ObjectId { get; set; }
            [Data] public short ActorId { get; set; }
            [Data] public short DeleteFlag { get; set; }
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{nameof(ReadMotion)}: ObjectEntry {ObjectId:X04}, ActorID {ActorId}, Unk? {DeleteFlag}, Path {Name}";
        }

        public class ReadAudio : IEventEntry
        {
            [Data] public string Name { get; set; }

            public override string ToString() => $"{nameof(ReadAudio)} {Name}";
        }

        public class SetShake : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Width { get; set; }
            [Data] public short Height { get; set; }
            [Data] public short Depth { get; set; }
            [Data] public short Duration { get; set; }
            [Data] public short Type { get; set; }

            public override string ToString() =>
                $"{nameof(SetShake)}: Frame {Frame}, Type {Type}, Width {Width}, Height {Height}, Depth {Depth}, Duration {Duration}";
        }

        public class StopEffect : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short PutId { get; set; }

            public override string ToString() =>
                $"{nameof(StopEffect)}: Frame {Frame}";
        }

        public class CacheClear : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data(Count = 96)] public byte[] PutId { get; set; }
        }

        public class SeqObjPause : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public byte Sw { get; set; }
            [Data] public byte Dummy { get; set; }
        }

        public class SeqBgse : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public byte Sw { get; set; }
            [Data] public byte Dummy { get; set; }

            public override string ToString() =>
                $"{nameof(SeqBgse)}: {Frame}, {Sw}, {Dummy}";
        }

        public class SeqGlow : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public byte Sw { get; set; }
            [Data] public byte Dummy { get; set; }
        }

        public class RunMovie : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public string Name { get; set; }

            public override string ToString() =>
                $"{nameof(RunMovie)}: Frame {Frame}, Name {Name}";
        }

        public class SeqSavePoint : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short PutId { get; set; }
        }

        public class SeqCameraCollision : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Type { get; set; }
        }

        public class SeqPosMove : IEventEntry
        {
            [Data] public short PutId { get; set; }
            [Data] public short Frame { get; set; }
            [Data] public float StartPositionX { get; set; }
            [Data] public float StartPositionY { get; set; }
            [Data] public float StartPositionZ { get; set; }
            [Data] public float EndPositionX { get; set; }
            [Data] public float EndPositionY { get; set; }
            [Data] public float EndPositionZ { get; set; }
            [Data] public float RotationX { get; set; }
            [Data] public float RotationY { get; set; }
            [Data] public float RotationZ { get; set; }
            [Data] public short Length { get; set; }
            [Data] public short Unk { get; set; }

            public override string ToString() =>
                $"{nameof(SeqPosMove)}: {PutId}, {Frame}, {Length}, {Unk}, StartPos({StartPositionX}, {StartPositionY}, {StartPositionZ}), EndPos({EndPositionX}, {EndPositionY}, {EndPositionZ}), Rot({RotationX}, {RotationY}, {RotationZ})";
        }

        public class BlackFog : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Length { get; set; } // This is originally analyzed as `short frame;` by Disgustor, however Frame already exists, Thus...
            [Data] public float Start { get; set; }
            [Data] public float End { get; set; }
        }

        public class Fog : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public byte Min { get; set; }
            [Data] public byte Max { get; set; }
            [Data] public int FogNear { get; set; }
            [Data] public byte R { get; set; }
            [Data] public byte G { get; set; }
            [Data] public byte B { get; set; }
            [Data] public byte Unk { get; set; }
        }

        public class PlayerOffsetCamera : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Type { get; set; }
        }

        public class SkyOff : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Type { get; set; }
        }

        public class SeqHideObject : IEventEntry
        {
            [Data] public short Frame { get; set; }
            [Data] public short Type { get; set; }

            public override string ToString() =>
                $"{nameof(SeqHideObject)}: Frame {Frame}, Type {Type}";
        }

        public class Light : IEventEntry
        {
            public short WorkNum { get; set; }
            public List<Data> LightData { get; set; }

            public class Data
            {
                [Data] public short PutId { get; set; }
                [Data] public short StartFrame { get; set; }
                [Data] public short EndFrame { get; set; }
                [Data] public byte CamNum { get; set; }
                [Data] public byte SubNum { get; set; }
                [Data] public LightParamPosition Position { get; set; }
            }

            public class LightParamPosition
            {
                [Data(Count = 9)] public float[] Pos { get; set; }
                [Data(Count = 12)] public float[] Color { get; set; }
            }

        }

        public class SeqMob : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short PutId { get; set; }
            [Data] public float X { get; set; }
            [Data] public float Y { get; set; }
            [Data] public float Z { get; set; }
            [Data] public int Num { get; set; }
            [Data] public float RangeX { get; set; }
            [Data] public float RangeY { get; set; }
            [Data] public float RangeZ { get; set; }
            [Data] public float RotY { get; set; }
        }

        public class Countdown : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
        }

        public class Tag : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short Unk { get; set; }
            [Data] public int TagNumber { get; set; }
        }

        public class WallClip : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short Flag { get; set; }
        }

        public class VoiceAllFadeout : IEventEntry
        {
            [Data] public short StartFrame { get; set; }
            [Data] public short FadeFrame { get; set; }
        }

        public static List<IEventEntry> Read(Stream stream)
        {
            var entries = new List<IEventEntry>();

            int blockLength;
            while (stream.Position + 3 < stream.Length)
            {
                blockLength = stream.ReadUInt16();
                if (blockLength == Terminator)
                    break;

                var startPosition = stream.Position - 2;
                var type = stream.ReadInt16();
                if (_idType.TryGetValue(type, out var entryType))
                    entries.Add(Mapping.ReadObject(stream, (IEventEntry)Activator.CreateInstance(entryType)));
                else
                    Console.Error.WriteLine($"No Event implementation for {type:X02}");

                if (stream.Position != startPosition + blockLength)
                    stream.Position = stream.Position;
                stream.Position = startPosition + blockLength;
            }

            return entries;
        }

        public static void Write(Stream stream,
            IEnumerable<IEventEntry> eventSet)
        {
            foreach (var item in eventSet)
            {
                var startPosition = stream.Position;
                stream.Position += 4;
                Mapping.WriteObject(stream, item);
                var nextPosition = stream.AlignPosition(2).Position;

                var id = _typeId[item.GetType()];
                var length = stream.Position - startPosition;
                stream.Position = startPosition;
                stream.Write((short)(length));
                stream.Write((short)id);
                stream.Position = nextPosition;
            }

            stream.Write(Terminator);
        }

        public static string ToString(IEnumerable<IEventEntry> eventSet) =>
            string.Join("\n", eventSet);

        private static string ReadCStyleString(Stream stream)
        {
            var sb = new StringBuilder();
            while (stream.Position < stream.Length)
            {
                var ch = stream.ReadByte();
                if (ch == 0)
                    break;

                sb.Append((char)ch);
            }

            return sb.ToString();
        }

        private static void WriteCStyleString(Stream stream, string str)
        {
            foreach (var ch in str)
                stream.WriteByte((byte)ch);
            stream.WriteByte(0);
        }

        private static void CStringWriter(MappingWriteArgs args) =>
            WriteCStyleString(args.Writer.BaseStream, (string)args.Item);

        private static object CStringReader(MappingReadArgs args) =>
            ReadCStyleString(args.Reader.BaseStream);

        private static object ReadVoices(MappingReadArgs args)
        {
            var voiceCount = args.Reader.ReadUInt16();
            args.Reader.ReadUInt16();
            var voices = new List<SeqVoices.Voice>(voiceCount);
            for (var i = 0; i < voiceCount; i++)
            {
                var voice = new SeqVoices.Voice();
                var startPos = args.Reader.BaseStream.Position;
                args.Reader.ReadInt32(); // It always supposed to be 0
                voice.FrameStart = args.Reader.ReadInt16();
                args.Reader.ReadByte();
                voice.Name = ReadCStyleString(args.Reader.BaseStream);
                voices.Add(voice);
                args.Reader.BaseStream.Position = startPos + 0x20;
            }

            return new SeqVoices
            {
                Voices = voices
            };
        }

        private static void WriteVoices(MappingWriteArgs args)
        {
            var item = (SeqVoices)args.Item;
            args.Writer.Write(item.Voices.Count);
            foreach (var voice in item.Voices)
            {
                var startPos = args.Writer.BaseStream.Position;
                args.Writer.Write(0);
                args.Writer.Write(voice.FrameStart);
                args.Writer.Write((byte)0);
                WriteCStyleString(args.Writer.BaseStream, voice.Name);
                args.Writer.BaseStream.Position = startPos + 0x20;
            }

            var endPosition = args.Writer.BaseStream.Position;
            if (endPosition > args.Writer.BaseStream.Length)
                args.Writer.BaseStream.SetLength(endPosition);
        }

        private static object ReadSetCameraData(MappingReadArgs args)
        {
            List<CameraKeys> AssignValues(SetCameraDataHeader header, IList<CameraKeys> val) =>
                Enumerable.Range(0, header.Count).Select(i => val[header.Index + i]).ToList();

            var cameraId = args.Reader.ReadInt16();
            var headers = Enumerable
                .Range(0, 8)
                .Select(x => BinaryMapping.ReadObject<SetCameraDataHeader>(args.Reader.BaseStream))
                .ToList();
            args.Reader.BaseStream.AlignPosition(4);
            var valueCount = headers.Max(x => x.Index + x.Count);
            var values = Enumerable
                .Range(0, valueCount)
                .Select(x => GetCameraKeys(BinaryMapping.ReadObject<CameraValueInternal>(args.Reader.BaseStream)))
                .ToList();

            return new SetCameraData
            {
                CameraId = cameraId,
                PositionX = AssignValues(headers[0], values),
                PositionY = AssignValues(headers[1], values),
                PositionZ = AssignValues(headers[2], values),
                LookAtX = AssignValues(headers[3], values),
                LookAtY = AssignValues(headers[4], values),
                LookAtZ = AssignValues(headers[5], values),
                Roll = AssignValues(headers[6], values),
                FieldOfView = AssignValues(headers[7], values),
            };
        }

        private static void WriteSetCameraData(MappingWriteArgs args)
        {
            void WriteHeader(Stream stream,
                List<CameraKeys> values,
                int sIndex) =>
                Mapping.WriteObject(stream, new SetCameraDataHeader
                {
                    Index = (short)sIndex,
                    Count = (short)values.Count
                });

            void WriteData(Stream stream, List<CameraKeys> values)
            {
                foreach (var value in values.Select(GetCameraValueInternal))
                    Mapping.WriteObject(stream, value);
            }


            var item = args.Item as SetCameraData;
            args.Writer.Write(item.CameraId);

            var channelCountList = new List<int>(8)
            {
                item.PositionX.Count,
                item.PositionY.Count,
                item.PositionZ.Count,
                item.LookAtX.Count,
                item.LookAtY.Count,
                item.LookAtZ.Count,
                item.Roll.Count,
                item.FieldOfView.Count,
            };
            var indexList = new List<int>(8);
            var startIndex = 0;
            foreach (var channelCount in channelCountList)
            {
                indexList.Add(startIndex);
                startIndex += channelCount;
            }

            WriteHeader(args.Writer.BaseStream, item.PositionX, indexList[0]);
            WriteHeader(args.Writer.BaseStream, item.PositionY, indexList[1]);
            WriteHeader(args.Writer.BaseStream, item.PositionZ, indexList[2]);
            WriteHeader(args.Writer.BaseStream, item.LookAtX, indexList[3]);
            WriteHeader(args.Writer.BaseStream, item.LookAtY, indexList[4]);
            WriteHeader(args.Writer.BaseStream, item.LookAtZ, indexList[5]);
            WriteHeader(args.Writer.BaseStream, item.Roll, indexList[6]);
            WriteHeader(args.Writer.BaseStream, item.FieldOfView, indexList[7]);

            args.Writer.BaseStream.AlignPosition(4);
            WriteData(args.Writer.BaseStream, item.PositionX);
            WriteData(args.Writer.BaseStream, item.PositionY);
            WriteData(args.Writer.BaseStream, item.PositionZ);
            WriteData(args.Writer.BaseStream, item.LookAtX);
            WriteData(args.Writer.BaseStream, item.LookAtY);
            WriteData(args.Writer.BaseStream, item.LookAtZ);
            WriteData(args.Writer.BaseStream, item.Roll);
            WriteData(args.Writer.BaseStream, item.FieldOfView);
        }

        private static object ReadLoadAssets(MappingReadArgs args)
        {
            var reader = args.Reader;
            var itemCount = reader.ReadInt16();
            var frameStart = reader.ReadInt16();
            var frameEnd = reader.ReadInt16();
            var unk06 = reader.ReadInt16();

            var loadSet = new List<IEventEntry>();
            for (var i = 0; i < itemCount; i++)
            {
                var startPosition = reader.BaseStream.Position;
                var typeId = reader.ReadInt16();
                var length = reader.ReadInt16();
                var entryType = _idType[typeId];
                loadSet.Add(Mapping.ReadObject(reader.BaseStream,
                    (IEventEntry)Activator.CreateInstance(entryType)));
                reader.BaseStream.Position = startPosition + length;
            }

            return new ReadAssets
            {
                FrameStart = frameStart,
                FrameEnd = frameEnd,
                Unk06 = unk06,
                Set = loadSet
            };
        }

        private static void WriteLoadAssets(MappingWriteArgs args)
        {
            var item = args.Item as ReadAssets;
            args.Writer.Write((short)item.Set.Count);
            args.Writer.Write(item.FrameStart);
            args.Writer.Write(item.FrameEnd);
            args.Writer.Write(item.Unk06);

            var stream = args.Writer.BaseStream;
            foreach (var loadItem in item.Set)
            {
                var startPosition = stream.Position;
                stream.Position += 4;
                Mapping.WriteObject(stream, loadItem);
                var nextPosition = stream.AlignPosition(2).Position;

                var id = _typeId[loadItem.GetType()];
                var length = stream.Position - startPosition;
                stream.Position = startPosition;
                stream.Write((short)id);
                stream.Write((short)length);
                stream.Position = nextPosition;
            }
        }

        private static object ReadSplinePoint(MappingReadArgs args)
        {
            var id = args.Reader.ReadInt16();
            var count = args.Reader.ReadInt16();
            var type = args.Reader.ReadInt16();
            var dummy = args.Reader.ReadInt16();
            var splineLng = args.Reader.ReadSingle();
            var entries = Enumerable
                .Range(0, count)
                .Select(x => Mapping.ReadObject<SplinePoint.Point>(args.Reader.BaseStream))
                .ToList();

            return new SplinePoint
            {
                Id = id,
                Type = type,
                SplineLng = splineLng,
                Points = entries
            };
        }

        private static void WriteSplinePoint(MappingWriteArgs args)
        {
            var item = args.Item as SplinePoint;
            args.Writer.Write((short)item.Id);
            args.Writer.Write((short)item.Points.Count);
            args.Writer.Write((short)item.Type);
            args.Writer.Write((short)0);
            args.Writer.Write(item.SplineLng);
            foreach (var entry in item.Points)
            {
                Mapping.WriteObject(args.Writer.BaseStream, entry);
            }
        }

        private static object ReadLight(MappingReadArgs args)
        {
            var count = args.Reader.ReadInt16();
            var workNum = args.Reader.ReadInt16();
            var lightData = Enumerable
                .Range(0, count)
                .Select(x => Mapping.ReadObject<Light.Data>(args.Reader.BaseStream))
                .ToList();

            return new Light
            {
                WorkNum = workNum,
                LightData = lightData,
            };
        }

        private static void WriteLight(MappingWriteArgs args)
        {
            var item = args.Item as Light;
            args.Writer.Write((short)(item.LightData.Count));
            args.Writer.Write(item.WorkNum);
            foreach (var one in item.LightData)
            {
                Mapping.WriteObject(args.Writer.BaseStream, one);
            }
        }

        private static object ReadSplineDataEnc(MappingReadArgs args)
        {
            var PutId = args.Reader.ReadInt16();
            var TransOfs = args.Reader.ReadInt16();
            var TransCnt = args.Reader.ReadInt16();
            args.Reader.ReadInt16();
            var keys = Enumerable
                .Range(0, TransCnt)
                .Select(x => GetCameraKeys(Mapping.ReadObject<CameraValueInternal>(args.Reader.BaseStream)))
                .ToList();

            return new SplineDataEnc
            {
                PutId = PutId,
                TransOfs = TransOfs,
                Keys = keys,
            };
        }

        private static void WriteSplineDataEnc(MappingWriteArgs args)
        {
            var item = args.Item as SplineDataEnc;
            args.Writer.Write(item.PutId);
            args.Writer.Write(item.TransOfs);
            args.Writer.Write((short)item.Keys.Count);
            args.Writer.Write((short)0);
            foreach (var one in item.Keys)
            {
                Mapping.WriteObject(args.Writer.BaseStream, GetCameraValueInternal(one));
            }
        }

        private static object ReadSeqPart(MappingReadArgs args)
        {
            args.Reader.BaseStream.Seek(-4, SeekOrigin.Current);
            var chunkSize = args.Reader.ReadInt16();
            args.Reader.ReadInt16();

            var startFrame = args.Reader.ReadInt16();
            var endFrame = args.Reader.ReadInt16();
            var putId = args.Reader.ReadInt16();
            var part = Enumerable
                .Range(0, (chunkSize - 10) / 2)
                .Select(x => args.Reader.BaseStream.ReadInt16())
                .ToArray();

            return new SeqPart
            {
                StartFrame = startFrame,
                EndFrame = endFrame,
                PutId = putId,
                Part = part,
            };
        }

        private static void WriteSeqPart(MappingWriteArgs args)
        {
            var item = args.Item as SeqPart;
            args.Writer.Write(item.StartFrame);
            args.Writer.Write(item.EndFrame);
            args.Writer.Write(item.PutId);
            foreach (var part in item.Part)
            {
                args.Writer.Write(part);
            }
        }

        private static object ReadVibData(MappingReadArgs args)
        {
            args.Reader.BaseStream.Seek(-4, SeekOrigin.Current);
            var chunkSize = args.Reader.ReadInt16();
            args.Reader.ReadInt16();

            var frame = args.Reader.ReadInt16();
            var dummy = args.Reader.ReadInt16();
            var data = args.Reader.BaseStream.ReadBytes(chunkSize - 8);

            return new VibData
            {
                Frame = frame,
                Dummy = dummy,
                Data = data,
            };
        }

        private static void WriteVibData(MappingWriteArgs args)
        {
            var item = args.Item as VibData;
            args.Writer.Write(item.Frame);
            args.Writer.Write(item.Dummy);
            if (item.Data != null)
            {
                args.Writer.Write(item.Data);
            }
        }
    }
}
