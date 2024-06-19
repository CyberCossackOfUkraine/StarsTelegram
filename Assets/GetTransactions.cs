using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GetTransactions : MonoBehaviour
{
    private string botToken = "7116863866:AAHl7VI9_g7FW8XtszUR2ZBdkgnwoA5ftM4"; // Replace with your actual bot token

    void Start()
    {
        StartCoroutine(GetStarTransactions(0, 100)); // Example usage
    }

    IEnumerator GetStarTransactions(int offset = 0, int limit = 100)
    {
        string URL = $"https://api.telegram.org/bot{botToken}/getStarTransactions";

        WWWForm form = new WWWForm();
        form.AddField("offset", offset);
        form.AddField("limit", limit);

        UnityWebRequest www = UnityWebRequest.Post(URL, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error getting star transactions: " + www.error);
            Debug.LogError(www.downloadHandler.text);
        }
        else
        {
            string jsonResponse = www.downloadHandler.text;
            Debug.Log("Received response: " + jsonResponse);

            StarTransactions starTransactions = JsonConvert.DeserializeObject<StarTransactions>(jsonResponse);
            Debug.Log("Number of transactions: " + starTransactions.transactions.Length);

            foreach (var transaction in starTransactions.transactions)
            {
                Debug.Log($"Transaction ID: {transaction.id}, Date: {transaction.date}, Amount: {transaction.amount}, Currency: {transaction.currency}");
            }
        }
    }
}
[System.Serializable]
public class StarTransactions
{
    public StarTransaction[] transactions;
}

[System.Serializable]
public class StarTransaction
{
    public int id;
    public string date;
    public string type;
    public int amount;
    public string currency;
    public User user;
    public string note;
}

[System.Serializable]
public class User
{
    public int id;
    public string first_name;
    public string last_name;
    public string username;
}
