using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;
using Newtonsoft.Json;

public class TelegramInvoice : MonoBehaviour
{
    private string botToken = "7116863866:AAHl7VI9_g7FW8XtszUR2ZBdkgnwoA5ftM4";
    private string chatId = "352292854";
    private string providerToken = "PROVIDER_TOKEN"; // This is your payment provider token

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => StartCoroutine(OpenPaymentWithoutInvoice()));
    }

    private IEnumerator OpenPaymentWithoutInvoice()
    {
        string url = $"https://api.telegram.org/bot{botToken}/sendInvoice";

        WWWForm form = new WWWForm();
        form.AddField("chat_id", chatId);
        form.AddField("title", "Title Test");
        form.AddField("description", "Description Test");
        form.AddField("payload", System.Guid.NewGuid().ToString());
        form.AddField("currency", "XTR");

        // Correctly format the prices field as a JSON array
        var prices = new List<LabeledPrice>
    {
        new LabeledPrice { label = "Price", amount = 1 }
    };
        string pricesJson = JsonConvert.SerializeObject(prices); // Use JsonConvert from Newtonsoft.Json
        form.AddField("prices", pricesJson);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error sending invoice: {www.error}");
            Debug.LogError(www.downloadHandler.text);
            yield break;
        }

        // Parse the response to get the invoice ID
        var response = JsonConvert.DeserializeObject<TelegramApiResponse>(www.downloadHandler.text);
        string invoiceId = response.result.invoice_id;

        // Send a request to answer the shipping query (if necessary)
        string answerShippingQueryUrl = $"https://api.telegram.org/bot{botToken}/answerShippingQuery?shipping_query_id=SHIPPING_QUERY_ID&ok=true";
        UnityWebRequest shippingQueryRequest = UnityWebRequest.Get(answerShippingQueryUrl);
        yield return shippingQueryRequest.SendWebRequest();

        // Send a request to answer the pre-checkout query
        string answerPreCheckoutQueryUrl = $"https://api.telegram.org/bot{botToken}/answerPreCheckoutQuery?pre_checkout_query_id=PRE_CHECKOUT_QUERY_ID&ok=true";
        UnityWebRequest preCheckoutQueryRequest = UnityWebRequest.Get(answerPreCheckoutQueryUrl);
        yield return preCheckoutQueryRequest.SendWebRequest();

        Debug.Log("Payment opened successfully!");
    }

    [System.Serializable]
    private class LabeledPrice
    {
        public string label;
        public int amount;
    }

    [System.Serializable]
    private class TelegramApiResponse
    {
        public bool ok;
        public TelegramApiResponseResult result;
    }

    [System.Serializable]
    private class TelegramApiResponseResult
    {
        public string invoice_id;
    }
}