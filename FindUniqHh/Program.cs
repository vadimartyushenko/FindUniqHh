using System.Globalization;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

if (args.Length == 0)
    Console.WriteLine("WTF?");
    
var srcFile = args.First();
var badHhText = File.ReadAllText(srcFile).Split("\n", StringSplitOptions.RemoveEmptyEntries);

var hhs = new List<Hh>();
var line = 0;
var parser = new Parser(@"^(PokerStars|Poker|GGPokerOk) Hand #.(\w+)");
var actParser = new Parser(@"^Hero - Seat \d: (\w+)\s([+-]?([0-9]*[.])?[0-9]+)");
while (line < badHhText.Length) {
    var lineText = badHhText[line];
    if (string.IsNullOrWhiteSpace(lineText))
    {
        line++;
        continue;
    }
        
    if (lineText.StartsWith("Hero -"))
    {
        var room = PokerNetwork.Unknown;
        var Id = string.Empty;
        var actType = string.Empty;
        var amount = 0.0f;
        var actResult = actParser.Rx(badHhText[line], g =>
        {
            actType = g[1].Value;
            amount = float.Parse(g[2].Value, CultureInfo.InvariantCulture);
        });
        var result = parser.Rx(badHhText[line + 3],g => {
            room = g[1].Value switch {
                "PokerStars" => PokerNetwork.PokerStars,
                "Poker" => PokerNetwork.GGPokerOk,
                "GGPokerOk" => PokerNetwork.GGPokerOk,
                _ => throw new Exception("Unknown room: " + g[1].Value)
            };
            Id = g[2].Value;
        });
        if (result != ParseResult.Done)
            throw new Exception("WTF???");
        var hh = new Hh(Id, new Action(actType, amount), badHhText[line + 1].StartsWith("BLUFF"), room);
        hhs.Add(hh);
        line += 4;
    }
    else
    {
        line++;
        continue;
    }
}

var uniqHhIds = new HashSet<(string, Action)>(capacity: 200);
var k = 0;
foreach (var hh in hhs)
{
    if (!uniqHhIds.Contains((hh.ID, hh.Act)))
    {
        uniqHhIds.Add((hh.ID, hh.Act));
        if (hh.isBluff)
            k++;
        Console.WriteLine(hh.ID);
    } else {
        //Console.WriteLine($"{hh.ID} - {hh.Act.Type}:{hh.Act.Amount:F2}");
    }
    
}
Console.WriteLine();

record Hh(string ID, Action Act, bool isBluff, PokerNetwork room);

record Action(string Type, float Amount);

class Parser
{
    private const RegexOptions RxOptions = RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled;
    
    public Regex Regex { get; }

    public Parser(string pattern)
    {
        Regex = new Regex(pattern, RxOptions);
    }
    public ParseResult Rx(string input, Action<GroupCollection> action)
    {
            var m = Regex.Match(input);
            if (m.Success) {
                action(m.Groups);
                return ParseResult.Done;
            }
            return ParseResult.Continue;

    }
}
public enum ParseResult
{
    Done,
    Continue,
    Reparse,
}

public enum PokerNetwork
{
    Unknown,
    Any,
    /// <summary>Full Tilt Poker</summary>
    FullTilt,
    /// <summary>Poker Stars</summary>
    PokerStars,
    /// <summary>iPoker Network (Playtech)</summary>
    IPoker,
    /// <summary>Ongame Network</summary>
    Ongame,
    /// <summary>PartyPoker</summary>
    PartyPoker,
    /// <summary>Absolute Poker</summary>
    AbsolutePoker,
    /// <summary>Pacific, 888, Cassava network</summary>
    Pacific,
    /// <summary>gamblergames.com</summary>
    GamblerGames,
    /// <summary>tonybetpoker.com</summary>
    TonyBet,
    /// <summary>fulpotpoker.com, chinese room</summary>
    FulPot,
    /// <summary>pokerdom.com, russian room</summary>
    PokerDom,
    /// <summary>bluffdaddy.com, indian room</summary>
    BluffDaddy,
    /// <summary>LianZhong.com, chinese room</summary>
    LianZhong,
    /// <summary>pppoker.net, chinese/indian room</summary>
    PPPoker,
    /// <summary>pokermaster.com, chinese app</summary>
    PokerMaster,
    /// <summary>kingsclubpkr.com/.net</summary>
    KingsClubPkr,
    /// <summary>upoker.net</summary>
    UPoker,
    /// <summary>blackchippoker.eu</summary>
    BlackChipPoker,
    /// <summary>ggpokerok.com</summary>
    GGPokerOk,
    /// <summary>pppoker clone</summary>
    KKPoker,
    /// <summary>supremapoker.net upoker clone</summary>
    SPoker,
    /// <summary>pokerbros.net upoker clone</summary>
    PokerBros,
    /// <summary>coinpoker.com TonyBet clone</summary>
    CoinPoker,
    /// <summary>x-poker.net PPPoker clone</summary>
    Xpoker,
    /// <summary>pokerrrrapp.com android app</summary>
    Poker2,
}
