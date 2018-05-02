using Facebook.Unity;
using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FBManager : MonoBehaviour
{
    [SerializeField] GameObject loginButton;
    [SerializeField] GameObject logoutButton;
    [SerializeField] Text profileName;
    [SerializeField] Image profileImage;

    void Awake()
    {
        if (!FB.IsInitialized)
            FB.Init(InitCallback, OnHideUnity);
        else
            FB.ActivateApp();
    }

    void InitCallback()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
            // do more with Facebook SDK
        }
        else
            Debug.Log("Failed to init the Facebook SDK ...");
    }

    public void FBLogin()
    {
        List<string> perms = new List<string>();
//        perms.Add("public_profile"); // default permission is public_profile
//        perms.Add("user_birthday");
//        perms.Add("email");
        FB.LogInWithReadPermissions(perms, LoginCallback);
    }

    void OnHideUnity(bool gameIsShown)
    {
        if (!gameIsShown)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }


    void LoginCallback(IResult result)
    {
        if (result.Error != null)
            Debug.Log(result.Error);
        else
        {
//            Debug.Log(FB.IsLoggedIn ? "Login success" : "Login failed");
            if (FB.IsLoggedIn)
            {
                var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
                foreach (string perm in aToken.Permissions)
                    Debug.Log(perm);
                Debug.Log("Login success");
            }
            else
                Debug.Log("Loing failed");
            DealWithFBLogin(FB.IsLoggedIn);
        }
    }

    void DealWithFBLogin(bool loggedIn)
    {
        if (loggedIn)
        {
            loginButton.SetActive(false);
            logoutButton.SetActive(true);
            string infoQuery = "/me?fields=name";
            string pictureQuery = "/me/picture?type=large";
            string friendQuery = "/me/friends";

            FB.API(infoQuery, HttpMethod.GET, GetFBInfo, new Dictionary<string, string>());
            //            FB.API("/me?fields=email", HttpMethod.GET, GetEmail, new Dictionary<string, string>());
            FB.API(pictureQuery, HttpMethod.GET, GetProfileImg); // type can be "square" or "large"
            FB.API(friendQuery, HttpMethod.GET, FriendsCallBack);
        }
        else
        {
            loginButton.SetActive(true);
            logoutButton.SetActive(false);
        }
    }

    void GetFBInfo(IResult result)
    {
        if (result.Error == null)
        {
            var info = result.ResultDictionary;
            profileName.text = info["name"].ToString();
        }
        else
            Debug.Log(result.Error);
    }

    void GetProfileImg(IGraphResult result)
    {
        if (result.Error == null)
            profileImage.sprite = Sprite.Create(result.Texture,
                new Rect(0, 0, result.Texture.width, result.Texture.height), new Vector2());
        else
            Debug.Log(result.Error);
    }

    public void FBLogout()
    {
        FB.LogOut();
        loginButton.SetActive(true);
        profileName.gameObject.SetActive(false);
        profileImage.DOFade(0, 1).SetEase(Ease.Linear).OnComplete(() => AfterFadeImg(gameObject));
    }

    void AfterFadeImg(GameObject go)
    {
        go.SetActive(false);
    }

    public void FBShare()
    {
        FB.ShareLink(new Uri("https://vnexpress.net/"), callback: ShareCallback);
    }

    void ShareCallback(IShareResult result)
    {
        if (result.Cancelled || !String.IsNullOrEmpty(result.Error))
            Debug.Log("ShareLink Error: " + result.Error);

        else if (!String.IsNullOrEmpty(result.PostId))
            // Print post identifier of the shared content
            Debug.Log(result.PostId);
        else
        {
            // Share succeeded without postID
            Debug.Log("ShareLink success!");
        }
    }

    public void FBInvite()
    {
//        FB.Mobile.AppInvite(new Uri("https://play.google.com/store/apps/details?id=com.resocoder.onecalc.calculator"));
        FB.AppRequest(message: "You should try this game.", title: "Check this super game");
    }

    public void FBRequest()
    {
        FB.AppRequest("Join me", title: "Play game");
    }

    void FriendsCallBack(IGraphResult result) // need permission 'user_friends'
    {
        Debug.Log(result.RawResult);
//        var data = (Dictionary<string, object>) Facebook.MiniJSON.Json.Deserialize(result.RawResult);
        IDictionary<string, object> data = result.ResultDictionary;
        List<object> friends = data["data"] as List<object>;
        foreach (var friend in friends)
        {
            var friendDic = friend as Dictionary<string, object>;
//            Debug.Log("Name: " + friendDic["name"] + " , ID: " + friendDic["id"]);

        }


        // debug name and id of friends
    }
}
