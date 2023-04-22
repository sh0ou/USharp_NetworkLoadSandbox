using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class LoadNetworkText : UdonSharpBehaviour
{
    [SerializeField, UdonSynced] public VRCUrl targetUrl;
    [SerializeField] private TextMeshProUGUI text_output;

    [Header("有効時自動ロード")]
    [SerializeField] private bool isLoadOnEnable;

    [Header("同期ロード")]
    [SerializeField] public bool isLoadSync;

    [Header("(op)InputField")]
    [SerializeField] VRCUrlInputField inputField;

    /// <summary>URL先のデータが読み込み済みかどうか</summary>
    [UdonSynced] private bool isUrlLoaded = false;

    private void OnEnable()
    {
        if (isLoadOnEnable)
        {
            if (targetUrl.Get().Length >= 1)
            {
                LoadURLText();
            }
        }
    }

    /// <summary>
    /// リンク先のテキストデータを読み込みます
    /// </summary>
    public void LoadURLText()
    {
        if (inputField && inputField.GetUrl() != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            targetUrl = inputField.GetUrl();
            RequestSerialization();
        }

        if (isLoadSync)
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, "OnLoadURLText");
        }
        else
        {
            OnLoadURLText();
        }
    }

    /// <summary>
    /// 画像を初期状態に戻します
    /// </summary>
    public void ResetImage()
    {
        isUrlLoaded = false;

        text_output.text = default;
    }

    public void OnLoadURLText()
    {
        VRCStringDownloader.LoadUrl(targetUrl, (IUdonEventReceiver)this);

        Debug.Log($"[<color=yellow>LoadNetworkText</color>]Loading... / {targetUrl}");
    }

    public override void OnStringLoadSuccess(IVRCStringDownload download)
    {
        Debug.Log($"[<color=green>LoadNetworkText</color>]Complete / {targetUrl}");

        text_output.text = download.Result;
        isUrlLoaded = true;
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        Debug.Log($"[<color=magenta>LoadNetworkText</color>]{result.ErrorCode} / {targetUrl}\n{result.Error}");
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (isUrlLoaded)
        {
            RequestSerialization();
            OnLoadURLText();
        }
    }
}
