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

    [Header("(op)url隠蔽文字数"), Tooltip("指定した文字数以降のURLを[*]で隠します\nDefault = 40")]
    [SerializeField] private int hideUrlLength = 40;

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

        var hideUrl = targetUrl.ToString().Substring(0, hideUrlLength);
        Debug.Log($"[<color=yellow>LoadNetworkText</color>]Loading... / {hideUrl + "**********"}");
    }

    public override void OnStringLoadSuccess(IVRCStringDownload download)
    {
        var hideUrl = download.Url.ToString().Substring(0, hideUrlLength);
        Debug.Log($"[<color=green>LoadNetworkText</color>]Complete / {hideUrl + "**********"}");

        text_output.text = download.Result;
        isUrlLoaded = true;
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        var hideUrl = result.Url.ToString().Substring(0, hideUrlLength);
        Debug.Log($"[<color=magenta>LoadNetworkText</color>]{result.ErrorCode} / {hideUrl + "**********"}\n{result.Error}");
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
