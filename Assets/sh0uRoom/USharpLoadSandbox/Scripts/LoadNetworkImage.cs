using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class LoadNetworkImage : UdonSharpBehaviour
{
    [SerializeField, UdonSynced] public VRCUrl targetUrl;
    [SerializeField] private Material mat_output;

    [Header("有効時自動ロード")]
    [SerializeField] private bool isLoadOnEnable;

    [Header("同期ロード")]
    [SerializeField] public bool isLoadSync;

    [Header("(op)InputField")]
    [SerializeField] VRCUrlInputField inputField;

    [Header("(op)url隠蔽文字数"), Tooltip("指定した文字数以降のURLを[*]で隠します\nDefault = 40")]
    [SerializeField] private int hideUrlLength = 40;

    [Header("(op)テクスチャ詳細設定")]
    [SerializeField] TextureInfo textureInfo;

    /// <summary>URL先のデータが読み込み済みかどうか</summary>
    [UdonSynced] private bool isUrlLoaded = false;

    private void OnEnable()
    {
        //起動時ロード
        if (isLoadOnEnable)
        {
            if (targetUrl.Get().Length >= 1)
            {
                LoadURLImage();
            }
        }
    }

    /// <summary>
    /// リンク先の画像データを読み込みます
    /// </summary>
    public void LoadURLImage()
    {
        //InputFieldからURLを取得
        if (inputField && inputField.GetUrl() != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            targetUrl = inputField.GetUrl();
            RequestSerialization();
        }

        //同期フラグに応じて読込
        if (isLoadSync)
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, "OnLoadURLImage");
        }
        else
        {
            OnLoadURLImage();
        }
    }

    /// <summary>
    /// 画像を初期状態に戻します
    /// </summary>
    public void ResetImage()
    {
        isUrlLoaded = false;

        mat_output.mainTexture = default;
    }

    public void OnLoadURLImage()
    {
        VRCImageDownloader downloader = new VRCImageDownloader();
        downloader.DownloadImage(targetUrl, mat_output, (IUdonEventReceiver)this, textureInfo);

        var hideUrl = targetUrl.ToString().Substring(0, hideUrlLength);
        Debug.Log($"[<color=yellow>LoadNetworkImage</color>]Loading... / {hideUrl + "**********"}");
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        var hideUrl = result.Url.ToString().Substring(0, hideUrlLength);
        Debug.Log($"[<color=green>LoadNetworkImage</color>]{result.State} / {hideUrl + "**********"}");

        if (result.State == VRCImageDownloadState.Complete)
        {
            if (mat_output.mainTexture == null)
            {
                mat_output.mainTexture = result.Result;
                isUrlLoaded = true;
            }
        }
    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
        var hideUrl = result.Url.ToString().Substring(0, hideUrlLength);
        Debug.Log($"[<color=magenta>LoadNetworkImage</color>]{result.Error} / {hideUrl + "**********"} / {result.ErrorMessage}");
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (isUrlLoaded)
        {
            RequestSerialization();
            OnLoadURLImage();
        }
    }
}
