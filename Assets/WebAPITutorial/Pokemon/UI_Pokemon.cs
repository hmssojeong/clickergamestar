using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Http;
using System;
using System.Linq;

public class UI_Pokemon : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private WebGetPokemon _pokemonDownloader;
    [SerializeField] private UI_PokemonDetail _pokemonDetailUI;

    [Header("UI")]
    [SerializeField] private Transform _contentParent;
    [SerializeField] private GameObject _pokemonUIPrefab;

    [Header("Search")]
    [SerializeField] private TMP_InputField _searchInput;
    [SerializeField] private Button _searchButton;

    [Header("Pagination")]
    [SerializeField] private Button _prevButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private TMP_Text _pageText;

    private int _currentPage = 1;
    private const int TOTAL_PAGES = 8;

    private HttpClient _imageClient = new HttpClient();
    private List<Pokemon> _allPokemon = new List<Pokemon>();
    private List<GameObject> _pokemonCards = new List<GameObject>();

    private static readonly Dictionary<string, Color> TypeColors = new Dictionary<string, Color>
    {
        { "grass",    new Color(0.38f, 0.73f, 0.42f) },  
        { "fire",     new Color(0.93f, 0.55f, 0.23f) },  
        { "water",    new Color(0.39f, 0.56f, 0.94f) },  
        { "bug",      new Color(0.65f, 0.76f, 0.10f) },  
        { "normal",   new Color(0.66f, 0.66f, 0.60f) },  
        { "poison",   new Color(0.64f, 0.35f, 0.73f) },  
        { "electric", new Color(0.97f, 0.82f, 0.17f) },  
        { "ground",   new Color(0.88f, 0.75f, 0.40f) },  
        { "fairy",    new Color(0.93f, 0.55f, 0.90f) },  
        { "fighting", new Color(0.76f, 0.20f, 0.16f) },  
        { "psychic",  new Color(0.98f, 0.33f, 0.53f) },  
        { "rock",     new Color(0.72f, 0.63f, 0.33f) },  
        { "ghost",    new Color(0.44f, 0.34f, 0.59f) },  
        { "ice",      new Color(0.59f, 0.85f, 0.84f) },  
        { "dragon",   new Color(0.44f, 0.21f, 0.97f) },  
        { "flying",   new Color(0.66f, 0.56f, 0.95f) }, 
        { "steel",    new Color(0.72f, 0.72f, 0.81f) }, 
    };

private async void Start()
    {
        if (_searchButton != null)
        { 
            _searchButton.onClick.AddListener(OnSearchClicked);
        }

        if (_searchInput != null)
        { 
            _searchInput.onSubmit.AddListener((_) => OnSearchClicked());
        }

        if (_prevButton != null) _prevButton.onClick.AddListener(OnPrevPage);
        if (_nextButton != null) _nextButton.onClick.AddListener(OnNextPage);

        await LoadPage(1);
    }

    private void OnSearchClicked()
    {
        if (_searchInput == null)
        {
            return;
        }
        string searchTerm = _searchInput.text.Trim().ToLower();
        FilterPokemon(searchTerm);
    }

    private void FilterPokemon(string searchTerm)
    {
        for (int i = 0; i < _pokemonCards.Count; i++)
        {
            if (_pokemonCards[i] == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(searchTerm))
            {
                _pokemonCards[i].SetActive(true);
            }
            else
            {
                bool match = _allPokemon[i].Name.ToLower().Contains(searchTerm)
                    || _allPokemon[i].Id.ToString().Contains(searchTerm)
                    || (!string.IsNullOrEmpty(_allPokemon[i].KoreanName) && _allPokemon[i].KoreanName.Contains(searchTerm));
                _pokemonCards[i].SetActive(match);
            }
        }
    }

    private void OnPokemonCardClicked(Pokemon pokemon)
    {
        if (_pokemonDetailUI != null)
        {
            _pokemonDetailUI.ShowDetail(pokemon);
        }
    }

    private async Task CreatePokemonUI(Pokemon data)
    {
        var go = Instantiate(_pokemonUIPrefab, _contentParent);
        _pokemonCards.Add(go);

        Button cardButton = go.GetComponent<Button>();
        if(cardButton != null)
        {
            Pokemon captured = data;
            cardButton.onClick.AddListener(() => OnPokemonCardClicked(captured));
        }

        Transform headerRow = go.transform.Find("HeaderRow");
        Transform badge = headerRow?.Find("NumberBadge");
        Transform badgeText = badge?.Find("BadgeText");
        Transform nameText = headerRow?.Find("NameText");
        Transform pokemonImage = go.transform.Find("PokemonImage");

        if (badgeText != null)
        {
            var badgeTMP = badgeText.GetComponent<TMP_Text>();
            if (badgeTMP != null)
            { 
                badgeTMP.text = data.Id.ToString("D3"); // 항상 3자리 숫자
            }
        }

        if (badge != null)
        {
            var badgeImg = badge.GetComponent<Image>();
            if (badgeImg != null)
            {
                string typeName = data.PrimaryType.ToLower();
                if (TypeColors.TryGetValue(typeName, out Color typeColor))
                { 
                    badgeImg.color = typeColor;
                }
                else
                { 
                    badgeImg.color = TypeColors["normal"];
                }
            }
        }

        if (nameText != null)
        {
            var nameTMP = nameText.GetComponent<TMP_Text>();
            if (nameTMP != null)
            {
                // 한국어 이름이 있으면 한국어, 없으면 영어
                nameTMP.text = !string.IsNullOrEmpty(data.KoreanName) ? data.KoreanName : data.Name;
            }
        }

        if (pokemonImage != null)
        {
            var rawImage = pokemonImage.GetComponent<RawImage>();
            if (rawImage != null && !string.IsNullOrEmpty(data.sprites?.Front_Default))
            {
                Texture2D pokemonTexture = await GetTextureFromUrl(data.sprites.Front_Default);
                if (pokemonTexture != null)
                {
                    pokemonTexture.filterMode = FilterMode.Point;
                    rawImage.texture = pokemonTexture;

                    // 이미지 비율 유지
                    AspectRatioFitter fitter = pokemonImage.GetComponent<AspectRatioFitter>();
                    if (fitter == null)
                    {
                        fitter = pokemonImage.gameObject.AddComponent<AspectRatioFitter>();
                    }
                    fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                    fitter.aspectRatio = (float)pokemonTexture.width / pokemonTexture.height;
                }
            }
        }
    }

    private async Task<Texture2D> GetTextureFromUrl(string url)
    {
        try
        {
            byte[] imageBytes = await _imageClient.GetByteArrayAsync(url);

            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes);

            return texture;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }


private async Task LoadPage(int page)
    {
        _currentPage = page;
        foreach (var card in _pokemonCards)
            if (card != null) Destroy(card);
        _pokemonCards.Clear();
        _allPokemon.Clear();
        if (_searchInput != null) _searchInput.text = "";
        if (_pageText != null) _pageText.text = $"{_currentPage} / {TOTAL_PAGES}";
        if (_prevButton != null) _prevButton.interactable = _currentPage > 1;
        if (_nextButton != null) _nextButton.interactable = _currentPage < TOTAL_PAGES;
        _allPokemon = await _pokemonDownloader.FetchPokemonDataAsync(_currentPage);
        foreach (var pokemon in _allPokemon)
            await CreatePokemonUI(pokemon);
    }

    private async void OnPrevPage() { if (_currentPage > 1) await LoadPage(_currentPage - 1); }
    private async void OnNextPage() { if (_currentPage < TOTAL_PAGES) await LoadPage(_currentPage + 1); }
}
