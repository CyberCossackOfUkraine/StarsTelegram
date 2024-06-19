using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CheckUpdates : MonoBehaviour
{
    private string botToken = "7116863866:AAHl7VI9_g7FW8XtszUR2ZBdkgnwoA5ftM4"; 

    void Start()
    {
        StartCoroutine(PollForUpdates());
    }

    IEnumerator PollForUpdates()
    {
        string URL = $"https://api.telegram.org/bot{botToken}/getUpdates";
        Debug.Log("Starting PollForUpdates");

        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get(URL);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
            }
            else
            {
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Received response: " + jsonResponse);

                UpdateWrapper updateWrapper = JsonConvert.DeserializeObject<UpdateWrapper>(jsonResponse);
                Debug.Log("Number of updates: " + updateWrapper.result.Length);

                foreach (var update in updateWrapper.result)
                {
                    if (update.pre_checkout_query != null)
                    {
                        HandlePreCheckoutQuery(update.pre_checkout_query);
                    }
                }
            }


            yield return new WaitForSeconds(1);
        }
    }

    void HandlePreCheckoutQuery(PreCheckoutQuery preCheckoutQuery)
    {
        // Log the pre-checkout query ID for debugging
        Debug.Log("Received pre_checkout_query_id: " + preCheckoutQuery.id);
        Debug.Log("Invoice Payload: " + preCheckoutQuery.invoice_payload);

        // Approve or reject the order based on your business logic
        bool isOrderApproved = true; // Replace with your actual business logic

        if (isOrderApproved)
        {
            StartCoroutine(AnswerPreCheckoutQuery(preCheckoutQuery.id, true, null));
        }
        else
        {
            StartCoroutine(AnswerPreCheckoutQuery(preCheckoutQuery.id, false, "Sorry, we are out of stock."));
        }
    }

    IEnumerator AnswerPreCheckoutQuery(string preCheckoutQueryId, bool ok, string errorMessage)
    {
        string URL = $"https://api.telegram.org/bot{botToken}/answerPreCheckoutQuery";

        WWWForm form = new WWWForm();
        form.AddField("pre_checkout_query_id", preCheckoutQueryId);
        form.AddField("ok", ok ? "true" : "false");

        if (!ok && errorMessage != null)
        {
            form.AddField("error_message", errorMessage);
        }

        UnityWebRequest www = UnityWebRequest.Post(URL, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error responding to pre-checkout query: " + www.error);
            Debug.LogError(www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Successfully responded to pre-checkout query");
        }
    }
}

[System.Serializable]
public class UpdateWrapper
{
    public UpdateList[] result;
}

[System.Serializable]
public class UpdateList
{
    public int update_id;
    public PreCheckoutQuery pre_checkout_query;
}

[System.Serializable]
public class PreCheckoutQuery
{
    public string id;
    public string currency;
    public int total_amount;
    public string invoice_payload;
    public string shipping_option_id;
}