﻿using TMPro;
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class LoadNetworkText : UdonSharpBehaviour
{
    [Header("読み込むURL")]
    [SerializeField, UdonSynced] public VRCUrl targetUrl;

    [SerializeField] public TextMeshProUGUI text_output;
    [HideInInspector] public string outputText;

    [Header("有効時自動ロード")]
    [SerializeField] private bool isLoadOnEnable;

    [Header("同期ロード")]
    [SerializeField] public bool isLoadSync;

    [Header("(op)InputField")]
    [SerializeField] private VRCUrlInputField inputField;

    [Header("(op)URL隠蔽文字数"), Tooltip("指定した文字数より後のURL文字列を隠蔽します\nDefault = 10")]
    [SerializeField] private int hideUrlLength = 10;

    /// <summary>URL先のデータが読み込み済みかどうか</summary>
    [UdonSynced, HideInInspector] public bool isUrlLoaded = false;

    private void OnEnable()
    {
        if (!text_output)
        {
            Debug.LogWarning($"[<color=yellow>LoadNetworkText</color>]出力先TextObjが指定されていません。値の保存のみ行われます - ObjName: {gameObject.name}");
        }
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

        if (text_output)
        {
            text_output.text = default;
        }
    }

    public void OnLoadURLText()
    {
        VRCStringDownloader.LoadUrl(targetUrl, (IUdonEventReceiver)this);

        if (hideUrlLength > targetUrl.Get().Length)
        {
            Debug.Log($"[<color=yellow>LoadNetworkText</color>]Loading... / {"https://********************"}");
        }
        else
        {
            var hideUrl = targetUrl.Get().Substring(0, hideUrlLength);
            Debug.Log($"[<color=yellow>LoadNetworkText</color>]Loading... / {hideUrl + "********************"}");
        }
    }

    public override void OnStringLoadSuccess(IVRCStringDownload download)
    {
        if (hideUrlLength > download.Url.ToString().Length)
        {
            Debug.Log($"[<color=green>LoadNetworkText</color>]Complete / {"https://********************"}");
        }
        else
        {
            var hideUrl = download.Url.ToString().Substring(0, hideUrlLength);
            Debug.Log($"[<color=green>LoadNetworkText</color>]Complete / {hideUrl + "********************"}");
        }

        if(outputText.Contains("<!DOCTYPE html>"))
        {
            Debug.LogError($"[<color=magenta>LoadNetworkText</color>]HTMLタグが含まれているため、読み込みを中止しました");
            return;
        }

        //結果出力
        outputText = download.Result;
        if (text_output)
        {
            text_output.text = outputText;
        }
        isUrlLoaded = true;
    }

    public override void OnStringLoadError(IVRCStringDownload result)
    {
        if (hideUrlLength > targetUrl.Get().Length)
        {
            Debug.Log($"[<color=magenta>LoadNetworkText</color>]{result.ErrorCode} / {"https://********************"}\n{result.Error}");
        }
        else
        {
            var hideUrl = targetUrl.Get().Substring(0, hideUrlLength);
            Debug.Log($"[<color=magenta>LoadNetworkText</color>]{result.ErrorCode} / {targetUrl}\n{result.Error}");
        }

        if (text_output)
        {
            text_output.text = result.Error;
        }
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
