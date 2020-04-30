/// <summary>
/// レイヤー名を定数で管理するクラス
/// </summary>
public static class LayerName
{
	public const int Default = 0;
	public const int TransparentFX = 1;
	public const int IgnoreRaycast = 2;
	public const int Water = 4;
	public const int UI = 5;
	public const int Ground = 8;
	public const int Sky = 9;
	public const int MoveObject = 10;
	public const int Puni = 11;
	public const int DefaultMask = 1;
	public const int TransparentFXMask = 2;
	public const int IgnoreRaycastMask = 4;
	public const int WaterMask = 16;
	public const int UIMask = 32;
	public const int GroundMask = 256;
	public const int SkyMask = 512;
	public const int MoveObjectMask = 1024;
	public const int PuniMask = 2048;
}
