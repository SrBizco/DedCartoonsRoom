using System;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class ExternalLinkState
{
    public bool CanOpen(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

public sealed class OpenExternalLink : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string url;

    private readonly ExternalLinkState state = new ExternalLinkState();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (state.CanOpen(url))
        {
            Application.OpenURL(url);
        }
    }
}
