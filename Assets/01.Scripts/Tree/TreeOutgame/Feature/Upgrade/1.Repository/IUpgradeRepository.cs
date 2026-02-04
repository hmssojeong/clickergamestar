using Cysharp.Threading.Tasks;

/// <summary>
/// 업그레이드 데이터 저장소 인터페이스
/// Local과 Firebase 구현체가 이 인터페이스를 따릅니다
/// </summary>
public interface IUpgradeRepository
{
    /// <summary>
    /// 업그레이드 데이터를 저장합니다
    /// </summary>
    UniTaskVoid Save(UpgradeSaveData data);

    /// <summary>
    /// 업그레이드 데이터를 불러옵니다
    /// </summary>
    UniTask<UpgradeSaveData> Load();
}