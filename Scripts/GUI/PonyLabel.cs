using Godot;
using System;
using System.Collections.Generic;

//2019 © Даниил Белов
//Создано 19.06.2019

///<summary>
///<para>Визуализатор текста, поддерживаемый форматирование тегами.</para>
///<para>В качестве шрифтов использует текстуры атласа</para>
///</summary>
public class PonyLabel : Control
{
    
    protected Vector2 _size = new Vector2(0, 0);

    public Vector2 Size {
        get { 
            return (!textNotUpdated || _textToShow != Text) ? _size : CalculateSize(); 
        }
    }


    //Отрсовка
    public override void _Draw() {
        if (_textToShow == null) return;
        if (_textToShow == "") return;

        List<float> linesWidths = null;
        if (Wrap) {
            linesWidths = GetLinesWidths(_textToShow);
        }

        string t = (Uppercase) ? _textToShow.ToUpper() : _textToShow;

        //Определение лексем
        string toShow = t.Replace(">", ">%nxt%").Replace("<", "%nxt%<");
        string[] lexemes = toShow.Split("%nxt%");
        
        float xOffset = 0;
        float xText = 0;
        float maxXText = 0;
        float yText = 0;
        float CurrentLineHeight = 0;

        int currentLine = 0;

        if (Wrap && Centered && linesWidths.Count != 0) xOffset = RectSize.x/2 - linesWidths[0]/2;

        //Словарь в формате название : количество открытых
        Dictionary<string, int> tags = new Dictionary<string, int>();
        
        foreach (string lexeme in lexemes) {
            //Если лексема это тег
            if (lexeme.Contains("<")) {
                string tagContent = lexeme.Replace("<", "").Replace(">", "");
                bool isClosing = tagContent.Contains("/"); //Является ли данный тег закрывающимся
                string tagName = tagContent.Replace("/", "").ToLower();

                //Обработка тегов не требуемых закрывания
                if (tagName == "br") {
                    if (Wrap) continue;
                    //GD.Print("BRRR");
                    xOffset = 0;
                    if (Centered && linesWidths.Count > currentLine+1) xOffset = RectSize.x/2 - linesWidths[currentLine+1]/2;
                    currentLine++;
                    xText = 0;
                    yText += CurrentLineHeight;
                    CurrentLineHeight = 0;
                    continue;
                }

                //обработка тегов, требуемых закрытие
                if (!tags.ContainsKey(tagName)) tags[tagName] = 0;
                tags[tagName] += (isClosing) ? -1 : 1;

                continue;
            } //Иначе

            //Определение шрифта и цвета на основании тегов
            string font = TextFont;
            Color color = FontColor;

            foreach (string key in tags.Keys) {
                if (tags[key] > 0) {
                    if (key.Contains("h")) {
                        int header = 0;
                        if (int.TryParse(key.Replace("h", ""), out header) && HeaderFonts.Length != 0) {
                            font = (HeaderFonts.Length >= header) ? HeaderFonts[header-1]
                                                                  : HeaderFonts[HeaderFonts.Length-1];
                        }
                    }
                    if (key.Contains("a")) {
                        int accent = 0;
                        if (int.TryParse(key.Replace("a", ""), out accent) && AccentColors.Length != 0) {
                            color = (AccentColors.Length >= accent) ? AccentColors[accent-1]
                                                                    : AccentColors[HeaderFonts.Length-1];
                        }
                    }
                    if (key == "b") font = BoldFont;
                    if (key == "s") font = StrokeFont;
                }
            }

            //Отрисовка части текста
            foreach (char c in lexeme) {
                if (Wrap) {
                    if (linesWidths.Count > currentLine && xText >= linesWidths[currentLine]) {
                        if (Centered && linesWidths.Count > currentLine+1) xOffset = RectSize.x/2 - linesWidths[currentLine+1]/2;
                        //GD.Print("[PonyLabel] NextLine");
                        currentLine++;
                        if (!Centered) xOffset = 0;
                        xText = 0;
                        yText += CurrentLineHeight;
                        CurrentLineHeight = 0;
                    }
                }
                Texture charTexture = GetCharTexture(font, c);
                if (charTexture == null) continue;
                //DrawTexture(charTexture, new Vector2(xText, yText), color);
                DrawTextureRect(charTexture, new Rect2(xText + xOffset, yText, charTexture.GetSize()), false, color);
                //При переносе убираем пробелы
                if (xText == 0 && Wrap && c == ' ') {
                    Vector2 tsize = charTexture.GetSize();
                    xOffset -= (Centered) ?  tsize.x / 2 + 3  :  tsize.x + 3;
                }
                xText += charTexture.GetSize().x + 3;
                if (maxXText < xText) maxXText = xText;

                if (charTexture.GetSize().y > CurrentLineHeight) CurrentLineHeight = charTexture.GetSize().y;
            }
        }

        _size.x = maxXText;
        _size.y = yText + CurrentLineHeight;

        textNotUpdated = false;
    }

    //Происходит ли анимация появления
    private bool showAnimationProcess = false;
    //Осталось милисекунд до след символа
    private float tick = 0;

    private string _textToShow = "";
    private string _text = "";
    [Export]
    ///Текст для отображения
    public string Text {
        get { return _text; }
        set {
            _text = value;
            _textToShow = _text;
            showAnimationProcess = false;
            textNotUpdated = true;

            Update();
        }
    }

    //Если текст ещё не обновился с помощью _Draw(), то true
    private bool textNotUpdated = false;

    private Color _fontColor = new Color(1, 1, 1, 1);
    [Export]
    public Color FontColor {
        get { return _fontColor; }
        set {
            _fontColor = value;
            textNotUpdated = true;
            Update();
        }
    }

    private Color[] _accentColors = new Color[0];
    [Export]
    public Color[] AccentColors {
        get { return _accentColors; }
        set { 
            _accentColors = value;
            textNotUpdated = true;
            Update();
        }
    }


    private bool _wrap = true;
    [Export]
    ///Нужно ли переносить слова на следующую строчку, если они не помещаются на строку по ширине
    public bool Wrap {
        get { return _wrap; }
        set {
            _wrap = value;
            textNotUpdated = true;
            Update();
        }
    }

    private bool _centered = true;
    [Export]
    ///Нужно ли переносить слова на следующую строчку, если они не помещаются на строку по ширине
    public bool Centered {
        get { return _centered; }
        set {
            _centered = value;
            textNotUpdated = true;
            Update();
        }
    }

    private bool _uppercase = true;
    [Export]
    ///Если установлено True, все буквы будут заглавными
    public bool Uppercase {
        get { return _uppercase; }
        set {
            _uppercase = value;
            textNotUpdated = true;
            Update();
        }
    }

    //Кеш текстур шрифтов
    //На данный момент чтобы движок не выгружал текстур
    //Из за чего был баг с прямоугольниками вместо текстур
    static Dictionary<string, Texture> FontsCash = new Dictionary<string, Texture>();

    public string[] _headerFonts = new string[0];
    [Export]
    public string[] HeaderFonts {
        get { return _headerFonts; }
        set {
            _headerFonts = value;
            textNotUpdated = true;
            Update();
        }
    }

    [Export]
    public string TextFont = "";

    [Export]
    public string BoldFont = "";

    [Export]
    public string StrokeFont = "";

    public void ShowText(bool animated) {
        Show();
        if (animated) {
            _textToShow = ""; showAnimationProcess = true;
        }
    }

    public void HideText(bool animated) {
        Hide();
    }

    ///Ищет текстуру символа (c) в указанной папке (path). Если текстуры нет, возвращает null
    private Texture GetCharTexture(string path, char c) {
        string charTexturePath = path+c.ToString()+".tres";
        Resource loaded = ResourceLoader.Load(charTexturePath);
        if (FontsCash.ContainsKey(charTexturePath))
            return FontsCash[charTexturePath];
        if (loaded != null && loaded is Texture) {
            Texture loadedTexture = loaded as Texture;
            loadedTexture.Flags = 0;
            if (!FontsCash.ContainsKey(charTexturePath))
                FontsCash[charTexturePath] = loadedTexture;
            return loadedTexture as Texture;
        }
        return null;
    }

    public override void _PhysicsProcess(float delta) {
        if (showAnimationProcess) {
            if (_text == "") {
                showAnimationProcess = false;
                return;
            }
            //GD.Print("[PonyLabel] Anim Process");
            if (tick <= 0) {
                int currentIndex = _textToShow.Length();
                char charToAdd = Text[currentIndex];
                _textToShow += charToAdd;
                currentIndex++;

                string tag = "";

                if (charToAdd == '<')
                    while (true) {
                        charToAdd = Text[currentIndex];
                        _textToShow += charToAdd;
                        tag += charToAdd;
                        currentIndex++;
                        if (charToAdd == '>' || _text == _textToShow) break;
                    }
                
                Update();

                //Установка задержки до следующего символа
                tick = 0.03f;
                if (charToAdd == ',') tick = 0.12f;
                if (charToAdd == '.' || charToAdd == '!' || charToAdd == '?' || tag == "w>") tick = 0.22f;

                if ( _text == _textToShow) showAnimationProcess = false;
            } else {
                tick -= delta;
            }
        }
    }

    private List<float> GetLinesWidths(string text) {
        List<float> widths = new List<float>();

        float wordWidth = 0;
        float lineWidth = 0;

        //Определение лексем
        string toShow = _textToShow.Replace(">", ">%nxt%").Replace("<", "%nxt%<");
        string[] lexemes = toShow.Split("%nxt%");

        float xText = 0;
        float yText = 0;
        float CurrentLineHeight = 0;

        //Словарь в формате название : количество открытых
        Dictionary<string, int> tags = new Dictionary<string, int>();
        
        foreach (string lexeme in lexemes) {
            //Если лексема это тег
            if (lexeme.Contains("<")) {
                string tagContent = lexeme.Replace("<", "").Replace(">", "");
                bool isClosing = tagContent.Contains("/"); //Является ли данный тег закрывающимся
                string tagName = tagContent.Replace("/", "");

                //Обработка тегов не требуемых закрывания
                if (tagName == "br") {
                    xText = 0;
                    yText += CurrentLineHeight;
                    CurrentLineHeight = 0;

                    if (lineWidth + wordWidth > RectSize.x) {
                        widths.Add(lineWidth);
                        lineWidth = wordWidth;
                        widths.Add(lineWidth);
                        wordWidth = 0;
                    } else {
                        lineWidth += wordWidth;
                        widths.Add(lineWidth);
                        wordWidth = 0;
                    }

                    continue;
                }

                //обработка тегов, требуемых закрытие
                if (!tags.ContainsKey(tagName)) tags[tagName] = 0;
                tags[tagName] += (isClosing) ? -1 : 1;

                continue;
            } //Иначе

            //Определение шрифта и цвета на основании тегов
            string font = TextFont;
            Color color = FontColor;

            foreach (string key in tags.Keys) {
                if (tags[key] > 0) {
                    if (key.Contains("h")) {
                        int header = 0;
                        if (int.TryParse(key.Replace("h", ""), out header) && HeaderFonts.Length != 0) {
                            font = (HeaderFonts.Length >= header) ? HeaderFonts[header-1]
                                                                  : HeaderFonts[HeaderFonts.Length-1];
                        }
                    }
                    if (key.Contains("a")) {
                        int accent = 0;
                        if (int.TryParse(key.Replace("a", ""), out accent) && AccentColors.Length != 0) {
                            color = (AccentColors.Length >= accent) ? AccentColors[accent-1]
                                                                    : AccentColors[HeaderFonts.Length-1];
                        }
                    }
                    if (key == "b") font = BoldFont;
                    if (key == "s") font = StrokeFont;
                }
            }

            //Обработка части текста
            foreach (char c in lexeme) {
                if (c == ' ') {
                    if (lineWidth + wordWidth > RectSize.x) {
                        widths.Add(lineWidth);
                        lineWidth = wordWidth;
                        wordWidth = 0;
                    } else {
                        lineWidth += wordWidth;
                        wordWidth = 0;
                    }
                }
                Texture charTexture = GetCharTexture(font, c);
                if (charTexture == null) continue;
                xText += charTexture.GetSize().x + 3;
                
                wordWidth += charTexture.GetSize().x + 3;

                if (charTexture.GetSize().y > CurrentLineHeight) CurrentLineHeight = charTexture.GetSize().y;
            }
        }

        if (lineWidth + wordWidth > RectSize.x) {
            widths.Add(lineWidth);
            lineWidth = wordWidth;
            widths.Add(lineWidth);
            wordWidth = 0;
        } else {
            lineWidth += wordWidth;
            widths.Add(lineWidth);
            wordWidth = 0;
        }

        return widths;
    }

    private Vector2 CalculateSize() {
        Vector2 size = new Vector2(0, 0);


        if (Text == "") return size;

        List<float> linesWidths = null;
        if (Wrap) {
            linesWidths = GetLinesWidths(Text);
        }

        //Определение лексем
        string toShow = Text.Replace(">", ">%nxt%").Replace("<", "%nxt%<");
        string[] lexemes = toShow.Split("%nxt%");
        
        float xOffset = 0;
        float xText = 0;
        float maxXText = 0;
        float yText = 0;
        float CurrentLineHeight = 0;

        int currentLine = 0;

        if (Wrap && Centered && linesWidths.Count != 0) xOffset = RectSize.x/2 - linesWidths[0]/2;

        //Словарь в формате название : количество открытых
        Dictionary<string, int> tags = new Dictionary<string, int>();
        
        foreach (string lexeme in lexemes) {
            //Если лексема это тег
            if (lexeme.Contains("<")) {
                string tagContent = lexeme.Replace("<", "").Replace(">", "");
                bool isClosing = tagContent.Contains("/"); //Является ли данный тег закрывающимся
                string tagName = tagContent.Replace("/", "");

                //Обработка тегов не требуемых закрывания
                if (tagName == "br") {
                    if (Wrap) continue;
                    //GD.Print("BRRR");
                    xOffset = 0;
                    if (Centered && linesWidths.Count > currentLine+1) xOffset = RectSize.x/2 - linesWidths[currentLine+1]/2;
                    currentLine++;
                    xText = 0;
                    yText += CurrentLineHeight;
                    CurrentLineHeight = 0;
                    continue;
                }

                //обработка тегов, требуемых закрытие
                if (!tags.ContainsKey(tagName)) tags[tagName] = 0;
                tags[tagName] += (isClosing) ? -1 : 1;

                continue;
            } //Иначе

            //Определение шрифта и цвета на основании тегов
            string font = TextFont;
            Color color = FontColor;

            foreach (string key in tags.Keys) {
                if (tags[key] > 0) {
                    if (key.Contains("h")) {
                        int header = 0;
                        if (int.TryParse(key.Replace("h", ""), out header) && HeaderFonts.Length != 0) {
                            font = (HeaderFonts.Length >= header) ? HeaderFonts[header-1]
                                                                  : HeaderFonts[HeaderFonts.Length-1];
                        }
                    }
                    if (key.Contains("a")) {
                        int accent = 0;
                        if (int.TryParse(key.Replace("a", ""), out accent) && AccentColors.Length != 0) {
                            color = (AccentColors.Length >= accent) ? AccentColors[accent-1]
                                                                    : AccentColors[HeaderFonts.Length-1];
                        }
                    }
                    if (key == "b") font = BoldFont;
                    if (key == "s") font = StrokeFont;
                }
            }

            //Отрисовка части текста
            foreach (char c in lexeme) {
                if (Wrap) {
                    if (linesWidths.Count > currentLine && xText >= linesWidths[currentLine]) {
                        if (Centered && linesWidths.Count > currentLine+1) xOffset = RectSize.x/2 - linesWidths[currentLine+1]/2;
                        //GD.Print("[PonyLabel] NextLine");
                        currentLine++;
                        if (!Centered) xOffset = 0;
                        xText = 0;
                        yText += CurrentLineHeight;
                        CurrentLineHeight = 0;
                    }
                }
                Texture charTexture = GetCharTexture(font, c);
                if (charTexture == null) continue;
                //При переносе убираем пробелы
                if (xText == 0 && Wrap && c == ' ') {
                    Vector2 tsize = charTexture.GetSize();
                    xOffset -= (Centered) ?  tsize.x / 2 + 3  :  tsize.x + 3;
                }
                xText += charTexture.GetSize().x + 3;
                if (maxXText < xText) maxXText = xText;

                if (charTexture.GetSize().y > CurrentLineHeight) CurrentLineHeight = charTexture.GetSize().y;
            }
        }

        size.x = maxXText;
        size.y = yText + CurrentLineHeight;


        return size;
    }


}


