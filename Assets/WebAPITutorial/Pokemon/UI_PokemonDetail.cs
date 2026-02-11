using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PokemonDetail : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private WebGetPokemon _pokemonDownloader;

    [Header("Popup")]
    [SerializeField] private GameObject _popupPanel;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Image _dimBackground;

    [Header("Basic Info")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private RawImage _pokemonImage;
    [SerializeField] private TMP_Text _heightValueText;
    [SerializeField] private TMP_Text _weightValueText;

    [Header("Type Badges")]
    [SerializeField] private Transform _typeBadgeParent;     
    [SerializeField] private GameObject _typeBadgePrefab;

    [Header("Stats")]
    [SerializeField] private Image[] _statFillBars;         
    [SerializeField] private TMP_Text[] _statValueTexts;    
    [SerializeField] private TMP_Text[] _statMaxTexts;     

    [Header("Description")]
    [SerializeField] private TMP_Text _descriptionText;

    [Header("Damage Relations")]
    [SerializeField] private TMP_Text _weakToText;          
    [SerializeField] private TMP_Text _resistantToText;   
    [SerializeField] private TMP_Text _immuneToText;

    private HttpClient _imageClient = new HttpClient();
    private const float STAT_MAX = 255f;

    private static readonly Dictionary<string, int> StatIndexMap = new Dictionary<string, int>
    {
        { "hp", 0 },
        { "attack", 1 },
        { "defense", 2 },
        { "special-attack", 3 },
        { "special-defense", 4 },
        { "speed", 5 }
    };

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

    private void Awake()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(HideDetail);
        }

        if (_dimBackground != null)
        {
            Button dimButton = _dimBackground.GetComponent<Button>();
            if (dimButton != null)
            {
                dimButton.onClick.AddListener(HideDetail);
            }
        }

        if (_popupPanel != null)
        {
            _popupPanel.SetActive(false);
        }
    }

    public async void ShowDetail(Pokemon pokemon)
    {
        if (pokemon == null)
        {
            return;
        }

        if(_dimBackground != null)
        {
            _dimBackground.gameObject.SetActive(true);
        }

        if(_popupPanel != null)
        {
            _popupPanel.SetActive(true);
        }

        BindBasicInfo(pokemon);
        BindStats(pokemon);
        await BindImage(pokemon);

        BindDescription(null); // 로딩 표시
        PokemonSpecies species = await _pokemonDownloader.FetchPokemonSpeciesAsync(pokemon.Id);
        BindDescription(species);

        BindDamageRelations(null); // 로딩 표시
        PokemonTypeData typeData = await _pokemonDownloader.FetchTypeDamageAsync(pokemon.PrimaryType);
        BindDamageRelations(typeData);
    }

    public void HideDetail()
    {
        if (_popupPanel != null)
        {
            _popupPanel.SetActive(false);
        }

        if (_dimBackground != null)
        {
            _dimBackground.gameObject.SetActive(false);
        }

        ClearTypeBadges();
    }

    private void BindBasicInfo(Pokemon pokemon)
    {
        if (_nameText != null)
        {
            _nameText.text = pokemon.Name;
        }

        if (_heightValueText != null)
        {
            _heightValueText.text = pokemon.HeightFormatted;
        }

        if (_weightValueText != null)
        {
            _weightValueText.text = pokemon.WeightFormatted;
        }

        // 타입 뱃지 생성
        ClearTypeBadges();

        if (pokemon.Types != null && _typeBadgeParent != null && _typeBadgePrefab != null)
        {
            foreach (var typeSlot in pokemon.Types)
            {
                CreateTypeBadge(typeSlot.type.Name);
            }
        }
    }

    private void CreateTypeBadge(string typeName)
    {
        GameObject badge = Instantiate(_typeBadgePrefab, _typeBadgeParent);

        // 뱃지 배경 색상
        Image badgeImage = badge.GetComponent<Image>();
        if (badgeImage != null)
        {
            string key = typeName.ToLower();
            badgeImage.color = TypeColors.ContainsKey(key) ? TypeColors[key] : TypeColors["normal"];
        }

        // 뱃지 텍스트
        TMP_Text badgeText = badge.GetComponentInChildren<TMP_Text>();
        if (badgeText != null)
        {
            badgeText.text = typeName;
        }
    }

    private void ClearTypeBadges()
    {
        if (_typeBadgeParent == null) return;

        for (int i = _typeBadgeParent.childCount - 1; i >= 0; i--)
        {
            Destroy(_typeBadgeParent.GetChild(i).gameObject);
        }
    }

    private void BindStats(Pokemon pokemon)
    {
        if (pokemon.Stats == null) return;

        foreach (var stat in pokemon.Stats)
        {
            string statName = stat.stat.Name.ToLower();
            if (!StatIndexMap.TryGetValue(statName, out int index)) continue;

            // 스탯 수치 텍스트
            if (_statValueTexts != null && index < _statValueTexts.Length && _statValueTexts[index] != null)
            {
                _statValueTexts[index].text = stat.BaseStat.ToString();
            }

            // 스탯 최대치 텍스트
            if (_statMaxTexts != null && index < _statMaxTexts.Length && _statMaxTexts[index] != null)
            {
                _statMaxTexts[index].text = ((int)STAT_MAX).ToString();
            }

            // 스탯 바 fillAmount (Image Type: Filled 필요)
            if (_statFillBars != null && index < _statFillBars.Length && _statFillBars[index] != null)
            {
                float fill = stat.BaseStat / STAT_MAX;
                _statFillBars[index].fillAmount = fill;

                // 스탯 바 색상을 주 타입 색상으로
                string typeKey = pokemon.PrimaryType.ToLower();
                if (TypeColors.TryGetValue(typeKey, out Color typeColor))
                {
                    _statFillBars[index].color = typeColor;
                }
            }
        }
    }

    private async Task BindImage(Pokemon pokemon)
    {
        if (_pokemonImage == null) return;

        string imageUrl = pokemon.sprites?.ArtworkUrl;
        if (string.IsNullOrEmpty(imageUrl)) return;

        Texture2D texture = await GetTextureFromUrl(imageUrl);
        if (texture != null)
        {
            texture.filterMode = FilterMode.Point;
            _pokemonImage.texture = texture;

            // 텍스처의 원래 비율을 유지
            AspectRatioFitter fitter = _pokemonImage.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                fitter.aspectRatio = (float)texture.width / texture.height;
            }
        }
    }

    private void BindDescription(PokemonSpecies species)
    {
        if (_descriptionText == null) return;

        if (species == null)
        {
            _descriptionText.text = "Loading...";
            return;
        }

        // 한국어 → 영어 순으로 시도
        string text = species.GetFlavorText("ko");
        if (string.IsNullOrEmpty(text))
        {
            text = species.GetFlavorText("en");
        }

        _descriptionText.text = !string.IsNullOrEmpty(text) ? text : "No description available.";
    }

    private void BindDamageRelations(PokemonTypeData typeData)
    {
        if (typeData == null)
        {
            SetDamageText(_weakToText, "Loading...");
            SetDamageText(_resistantToText, "Loading...");
            SetDamageText(_immuneToText, "Loading...");
            return;
        }

        var relations = typeData.DamageRelations;

        SetDamageText(_weakToText, FormatTypeList(relations.DoubleDamageFrom));
        SetDamageText(_resistantToText, FormatTypeList(relations.HalfDamageFrom));
        SetDamageText(_immuneToText, FormatTypeList(relations.NoDamageFrom));
    }

    private void SetDamageText(TMP_Text textField, string value)
    {
        if (textField != null)
        {
            textField.text = value;
        }
    }

    private string FormatTypeList(List<NamedResource> types)
    {
        if (types == null || types.Count == 0) return "None";

        List<string> names = new List<string>();
        foreach (var t in types)
        {
            // 첫 글자 대문자
            string name = char.ToUpper(t.Name[0]) + t.Name.Substring(1);
            names.Add(name);
        }
        return string.Join(", ", names);
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
            Debug.Log($"Image download failed: {e.Message}");
            return null;
        }
    }


}
