using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDKBase.Editor.Attributes;

/// <summary>
/// サーバーテキストからVRChatIDを判断し、当てはまったIDのみギミックを発動させるスクリプト
/// </summary>
public class UserIDGimmickLauncher : UdonSharpBehaviour
{
    [Header("参照するテキストスクリプト")]
    [HelpBox("LoadNetworkTextのtargetURLからテキストを取得します\nテキストファイル内のVRChatIDはカンマ（,）区切りで指定して下さい", HelpBoxAttribute.MessageType.Info)]
    [SerializeField] LoadNetworkText LoadNetworkText;
    private string[] targetUserIDs;

    [Header("アクション設定")]
    [HelpBox("アタッチ状況に応じてアクションを決定します\n使用するアクションのみアタッチして下さい\n*用途は変数名にマウスをあてることで表示されます", HelpBoxAttribute.MessageType.Info)]
    [SerializeField, Tooltip("テレポート - 対象位置")] private Transform targetTeleportPos;
    [SerializeField, Tooltip("オブジェクト表示切替 - 対象Obj")] private GameObject targetShowObj;
    [SerializeField, Tooltip("アニメーション - Animator")] private Animator targetAnimator;
    [SerializeField, Tooltip("アニメーション - アニメーション名")] private string targetAnimationName;

    private bool isLoaded = false;

    private void Start()
    {
        if (!LoadNetworkText) LoadNetworkText = GetComponent<LoadNetworkText>();
    }

    private void Update()
    {
        if (!isLoaded)
        {
            //データ取得チェック
            if (LoadNetworkText.outputText != "")
            {
                ConvertStringToArray(LoadNetworkText.outputText);
                Debug.Log($"[<color=green>UserIDGimmickLauncher</color>]データ取得完了 - {gameObject.name}");
                isLoaded = true;
            }
        }
    }

    /// <summary>
    /// テキストを配列に変換します
    /// </summary>
    public void ConvertStringToArray(string str) => targetUserIDs = str.Split(',');

    public override void Interact()
    {
        if (!CheckUserID()) return;

        if (targetTeleportPos)
        {
            Teleport();
        }
        else if (targetShowObj)
        {
            ToggleShowObj();
        }
        else if (targetAnimator && targetAnimationName != "")
        {
            PlayAnimation();
        }
        else
        {
            Debug.LogError("[<color=magenta>UserIDGimmickLauncher</color>]コンポーネントが1つもアタッチされてません");
        }
    }

    /// <summary>
    /// ユーザーIDと配列を照合します
    /// </summary>
    public bool CheckUserID()
    {
        foreach (var userID in targetUserIDs)
        {
            if (userID == Networking.LocalPlayer.displayName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 自身を対象位置に移動させます
    /// </summary>
    public void Teleport() => Networking.LocalPlayer.TeleportTo(targetTeleportPos.position, targetTeleportPos.rotation);

    /// <summary>
    /// オブジェクト表示状態を切り替えます
    /// </summary>
    public void ToggleShowObj() => targetShowObj.SetActive(!targetShowObj.activeSelf);

    /// <summary>
    /// アニメーションを再生します
    /// </summary>
    public void PlayAnimation() => targetAnimator.Play(targetAnimationName);
}
