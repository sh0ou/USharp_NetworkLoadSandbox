using TMPro;
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

    [Header("(op)ロード状況表示テキスト")]
    [SerializeField] TextMeshProUGUI text_loadInfo;

    [Header("(op)テクスチャ詳細設定")]
    [SerializeField] TextureInfo textureInfo;

    [Header("(op)URL隠蔽文字数"), Tooltip("指定した文字数より後のURL文字列を隠蔽します\nDefault = 40")]
    [SerializeField] private int hideUrlLength = 40;

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

    private void Update()
    {

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

        if (text_loadInfo) text_loadInfo.text = "Ready";
    }

    public void OnLoadURLImage()
    {
        VRCImageDownloader downloader = new VRCImageDownloader();
        downloader.DownloadImage(targetUrl, mat_output, (IUdonEventReceiver)this, textureInfo);

        //ログ出力
        if (hideUrlLength > targetUrl.Get().Length)
        {
            Debug.Log($"[<color=yellow>LoadNetworkImage</color>]Loading... / {"https://********************"}");
        }
        else
        {
            var hideUrl = targetUrl.Get().Substring(0, hideUrlLength);
            Debug.Log($"[<color=yellow>LoadNetworkImage</color>]Loading... / {hideUrl + "********************"}");
        }

        if (text_loadInfo) text_loadInfo.text = "Loading...";
    }

    public override void OnImageLoadSuccess(IVRCImageDownload result)
    {
        //ログ出力
        if (hideUrlLength > targetUrl.Get().Length)
        {
            Debug.Log($"[<color=green>LoadNetworkImage</color>]{result.State} / {"https://********************"}");
        }
        else
        {
            var hideUrl = targetUrl.Get().Substring(0, hideUrlLength);
            Debug.Log($"[<color=green>LoadNetworkImage</color>]{result.State} / {hideUrl + "********************"}");
        }

        if (result.State == VRCImageDownloadState.Complete)
        {
            if (mat_output.mainTexture == null)
            {
                mat_output.mainTexture = result.Result;
                isUrlLoaded = true;
            }
        }

        if (text_loadInfo) text_loadInfo.text = result.State.ToString();
    }

    public override void OnImageLoadError(IVRCImageDownload result)
    {
        //ログ出力
        if (hideUrlLength > targetUrl.Get().Length)
        {
            Debug.Log($"[<color=magenta>LoadNetworkImage</color>]{result.Error} / {"https://********************"} / {result.ErrorMessage}");
        }
        else
        {
            var hideUrl = targetUrl.Get().ToString().Substring(0, hideUrlLength);
            Debug.Log($"[<color=magenta>LoadNetworkImage</color>]{result.Error} / {hideUrl + "********************"} / {result.ErrorMessage}");
        }

        if (text_loadInfo) text_loadInfo.text = result.ErrorMessage;
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
