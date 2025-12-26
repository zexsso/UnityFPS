using System.Collections;
using System.Collections.Generic;
using PurrNet;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays kill notifications in a feed.
/// Shows who killed who with what weapon.
/// </summary>
public class KillFeedView : View
{
    [Header("Settings")]
    [SerializeField] private int maxEntries = 5;
    [SerializeField] private float entryDisplayTime = 5f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("References")]
    [SerializeField] private Transform entriesParent;
    [SerializeField] private KillFeedEntry entryPrefab;

    private Queue<KillFeedEntry> _activeEntries = new();
    private Queue<KillFeedEntry> _entryPool = new();

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);

        // Pre-populate pool
        for (int i = 0; i < maxEntries; i++)
        {
            var entry = Instantiate(entryPrefab, entriesParent);
            entry.gameObject.SetActive(false);
            _entryPool.Enqueue(entry);
        }
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<KillFeedView>();
    }

    /// <summary>
    /// Adds a kill notification to the feed
    /// </summary>
    public void AddKillEntry(string killerName, string victimName, bool isHeadshot = false)
    {
        // Remove oldest entry if at max
        if (_activeEntries.Count >= maxEntries)
        {
            var oldEntry = _activeEntries.Dequeue();
            ReturnEntryToPool(oldEntry);
        }

        // Get entry from pool or create new
        KillFeedEntry entry;
        if (_entryPool.Count > 0)
        {
            entry = _entryPool.Dequeue();
        }
        else
        {
            entry = Instantiate(entryPrefab, entriesParent);
        }

        entry.gameObject.SetActive(true);
        entry.transform.SetAsLastSibling();
        entry.SetData(killerName, victimName, isHeadshot);

        _activeEntries.Enqueue(entry);

        // Start fade out timer
        StartCoroutine(FadeOutEntry(entry, entryDisplayTime));
    }

    private IEnumerator FadeOutEntry(KillFeedEntry entry, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Fade out
        float elapsed = 0f;
        CanvasGroup cg = entry.GetComponent<CanvasGroup>();
        if (cg == null) cg = entry.gameObject.AddComponent<CanvasGroup>();

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = 1f - (elapsed / fadeOutDuration);
            yield return null;
        }

        // Return to pool
        if (_activeEntries.Contains(entry))
        {
            // Remove from active queue (rebuild queue without this entry)
            var tempQueue = new Queue<KillFeedEntry>();
            while (_activeEntries.Count > 0)
            {
                var e = _activeEntries.Dequeue();
                if (e != entry)
                    tempQueue.Enqueue(e);
            }
            _activeEntries = tempQueue;
        }

        ReturnEntryToPool(entry);
    }

    private void ReturnEntryToPool(KillFeedEntry entry)
    {
        if (entry == null) return;

        entry.gameObject.SetActive(false);

        var cg = entry.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;

        _entryPool.Enqueue(entry);
    }

    public override void OnShow() { }
    public override void OnHide() { }
}
