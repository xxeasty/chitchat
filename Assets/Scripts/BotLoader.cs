using Firebase.Firestore;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Firebase;

public class BotData
{
    public string BotId;
    public string Nickname;
}

public class BotLoader : MonoBehaviour
{
    FirebaseFirestore db;

    public async Task LoadBotsAndCreateButtons(string userId, System.Action<List<BotData>> onBotsLoaded)
    {

        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus != DependencyStatus.Available)
        {
            Debug.LogError("Firebase dependencies not resolved: " + dependencyStatus);
            return;
        }

        db = FirebaseFirestore.DefaultInstance;

        CollectionReference botsRef = db.Collection("users").Document(userId).Collection("bots");

        QuerySnapshot snapshot;
        try
        {
            snapshot = await botsRef.GetSnapshotAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"봇 목록 로드 실패: {e}");
            return;
        }

        List<BotData> botList = new List<BotData>();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            string botId = doc.Id;
            Dictionary<string, object> data = doc.ToDictionary();

            string nickname = data.ContainsKey("nickname") ? data["nickname"].ToString() : botId;

            botList.Add(new BotData
            {
                BotId = botId,
                Nickname = nickname
            });
        }

        onBotsLoaded?.Invoke(botList);
    }
}