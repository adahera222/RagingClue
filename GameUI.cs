using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GameUI : MonoBehaviour {
	static string setupText = "Welcome to Raging Clue, the fast-paced high-stakes game where everyone is a loser. Just kidding, you're great!\n\nI'm your host, Smirky McChimpsalot, and in case you were wondering -- yes, I'm naked.\n\nWhat was your name again?";

	static string defaultWelcomeText = "Welcome to the fray, <% PLAYERNAME %>!";
	static string welcomeText;

	public Texture2D logo;

	public TextAsset rules;
	public TextAsset credits;

	public GUISkin mySkin;

	static int buttonWidth = 204;

	Rect areaRect;

	void Awake() {
		areaRect = new Rect();
	}

	void Update() {
		if ( Game.displayFunds < Game.funds ) {
			Game.displayFunds = (int)Mathf.Clamp(Game.displayFunds + Random.Range(1, Game.funds * .1f), Game.displayFunds, Game.funds);
		} else if ( Game.displayFunds > Game.funds ) {
			Game.displayFunds = (int)Mathf.Clamp(Game.displayFunds - Random.Range(1, Game.displayFunds * .1f), Game.funds, Game.displayFunds);
		}

		// areaRect.x = Screen.width * .5f - 300;
		areaRect.x = Screen.width - 700;
		// areaRect.y = Screen.height * .5f - 200;
		areaRect.y = 150;
		areaRect.width = 645;
		// areaRect.height = 400;
		areaRect.height = Screen.height - 200;
	}

	void OnGUI() {
		GUI.skin = mySkin;

		GUI.Box(new Rect(areaRect.x - 20, areaRect.y - 20, areaRect.width + 40, areaRect.height + 40), string.Empty);

		switch(Game.gameState) {
			case GameState.PlayerSetup:
				DrawPlayerSetup();
				break;

			case GameState.Welcome:
				DrawWelcome();
				break;

			case GameState.ChooseCategoryAndWager:
				DrawChooseCategory();
				break;

			case GameState.DisplayClue:
				DrawClue();
				break;

			case GameState.WittyBanter:
				DrawWittyBanter();
				break;

			case GameState.GameOver:
				DrawGameOver();
				break;

			case GameState.Credits:
				DrawCredits();
				break;
		}

		if ( Game.gameState != GameState.PlayerSetup ) {
			DrawValue(5, 5, "Internet Dollars:", "$" + Game.displayFunds.ToString(), Color.green, false);
			DrawValue(Screen.width - 205, 5, "Round:", Game.round + "/" + Game.maxRounds, Color.cyan, false);
			if ( Game.stars < 1.0f ) {
				Color color = Color.black;
				color.a = .25f;
				DrawValue(Screen.width - 410, 5, "Star Rating:", "★", color, false);
			} else {
				DrawValue(Screen.width - 410, 5, "Star Rating:", Game.rating, Color.yellow, false);
			}
		}
	}

	void DrawPlayerSetup() {
		GUILayout.BeginArea(areaRect);

		GUILayout.Label(setupText);

		GUILayout.Space(12);

		Game.playerName = GUILayout.TextField(Game.playerName, GUILayout.Width(250));
		Game.playerName = Regex.Replace(Game.playerName, "\\W", "");

		if ( DrawConfirmButton("Let's Go!", (Game.playerName.Length != 0)) ) {
			welcomeText = Regex.Replace(defaultWelcomeText, "<% PLAYERNAME %>", Game.playerName);
			Game.SetGameState(GameState.Welcome);
		}

		GUILayout.EndArea();

		GUI.color = Color.black;

		GUILayout.BeginArea(new Rect(5, Screen.height-405, 400, 400));
		GUILayout.FlexibleSpace();
		GUILayout.Label("Raging Clue v0.1");
		GUILayout.Space(-6);
		GUILayout.Label("Made in 48 hours for Ludum Dare 27", "Small");
		GUILayout.Label("©2013 Alex Ayars <aayars@gmail.com>", "Small");
		GUILayout.EndArea();

		GUI.color = Color.white;

		GUILayout.BeginArea(new Rect(6, Screen.height-404, 400, 400));
		GUILayout.FlexibleSpace();
		GUILayout.Label("Raging Clue v0.1");
		GUILayout.Space(-6);
		GUILayout.Label("Made in 48 hours for Ludum Dare 27", "Small");
		GUILayout.Label("©2013 Alex Ayars <aayars@gmail.com>", "Small");
		GUILayout.EndArea();

		GUILayout.BeginArea(new Rect(5, 5, 500, 400));
		GUILayout.Label(logo, GUILayout.Width(200), GUILayout.Height(150));
		GUILayout.EndArea();
	}

	void DrawWelcome() {
		GUILayout.BeginArea(areaRect);

		GUILayout.Label(welcomeText);

		GUILayout.Space(8);

		GUILayout.Label(rules.text);

		if ( DrawConfirmButton("Whatever", true) ) {
			Game.SetGameState(GameState.ChooseCategoryAndWager);
		}

		GUILayout.EndArea();
	}

	void DrawChooseCategory() {
		GUILayout.BeginArea(areaRect);

		GUILayout.Label("Category: " + Game.categoryNames[Game.category], "Clue", GUILayout.ExpandWidth(true));

		if ( Game.round == Game.maxRounds ) {
			GUILayout.Label("This is the final round! DOUBLE OR NOTHING!");

			GUILayout.Label("All of your Internet Dollars are on the table for this one, " + Game.playerName + ".");

			Game.wager = Game.funds;

		} else {
			GUILayout.BeginHorizontal();
			GUILayout.Label("Your Wager:", GUILayout.Width(100));

			GUILayout.BeginVertical();
			GUILayout.Space(10);
			int max = Mathf.Clamp(Game.funds, 1, 25000);
			// Need Mathf.Clamp too because Unity
			Game.wager = (int)Mathf.Clamp(GUILayout.HorizontalSlider(Game.wager, 1, max, GUILayout.ExpandWidth(true)), 1, max);
			GUILayout.EndVertical();
			GUILayout.Label("$" + Game.wager.ToString(), "SmallValue", GUILayout.Width(100));
			GUILayout.EndHorizontal();
		}

		if ( DrawConfirmButton("Go!", (Game.category != Category.None)) ) {
			Game.SetGameState(GameState.ChooseClue);
		}

		GUILayout.EndArea();

		DrawValue(210, 5, "Your Wager:", "$" + Game.wager.ToString(), Color.yellow, false);

	}

	void DrawClue() {
		GUILayout.BeginArea(areaRect);

		GUILayout.Label("Category: " + Game.categoryNames[Game.category], "Clue", GUILayout.ExpandWidth(true));

		GUILayout.Label("Clues:");

		GUILayout.BeginHorizontal();
		int i = 0;
		foreach ( string clue in Game.clueInstance.clues ) {
			if ( i != 0 && i % 3 == 0 ) {
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}
			GUILayout.Label(clue, "Clue", GUILayout.Width(buttonWidth));
			i++;
		}
		GUILayout.EndHorizontal();

		GUILayout.Space(10);

		GUILayout.Label("Your Answer?");

		GUILayout.BeginHorizontal();
		i = 0;
		foreach ( string choice in Game.clueInstance.choices ) {
			if ( i != 0 && i % 3 == 0 ) {
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}
			if ( GUILayout.Button(choice, GUILayout.Width(buttonWidth) ) ) {
				Game.choice = i;
				Game.SetGameState(GameState.SubmitAnswer);
			}
			i++;
		}
		GUILayout.EndHorizontal();

		GUILayout.EndArea();

		DrawValue(210, 5, "Your Wager:", "$" + Game.wager.ToString(), Color.yellow, false);

		Color color;
		if ( Game.timeRemaining > Game.timePerRound * .5f ) {
			color = Color.Lerp(Color.yellow, Color.green, ( Game.timeRemaining - Game.timePerRound * .5f ) / 5.0f);
		} else {
			color = Color.Lerp(Color.red, Color.yellow, Game.timeRemaining / 5.0f);
		}
		DrawValue((int)(Screen.width * .5f - 100), 5, "Time:", Mathf.Ceil(Game.timeRemaining).ToString(), color, true);
	}

	void DrawWittyBanter() {
		GUILayout.BeginArea(areaRect);

		switch(Game.banterState) {
			case BanterState.RightAnswer:
				GUILayout.Label("Correct Answer!", "Clue", GUILayout.ExpandWidth(true));
				break;

			case BanterState.WrongAnswer:
				GUILayout.Label("Wrong Answer!", "Clue", GUILayout.ExpandWidth(true));
				break;

			case BanterState.TimeExpired:
				GUILayout.Label("Time Expired!", "Clue", GUILayout.ExpandWidth(true));
				break;
		}

		GUILayout.Label(Game.banter);

		GUILayout.Space(8);

		GUILayout.Label(Game.clueInstance.explanation);

		if ( DrawConfirmButton("Whatever", true) ) {
			Game.SetGameState(GameState.ChooseCategoryAndWager);
		}

		GUILayout.EndArea();
	}	

	void DrawGameOver() {
		GUILayout.BeginArea(areaRect);

		if ( Game.banterState == BanterState.LostGame ) {
			GUILayout.Label("You are dead.\n\nI told you we'd shoot!");
		} else {
			GUILayout.Label("Congratulations, you win the game! About those Internet Dollars... we might have to pay you next week. We're good for it.");
			GUILayout.Label("See you next time, " + Game.playerName + "!");
		}

		if ( DrawConfirmButton("Whatever", true) ) {
			Game.SetGameState(GameState.Credits);
		}

		GUILayout.EndArea();
	}

	void DrawCredits() {
		GUILayout.BeginArea(areaRect);

		GUILayout.Label(credits.text);

		if ( DrawConfirmButton("Whatever", true) ) {
			Game.SetGameState(GameState.PlayerSetup);
		}

		GUILayout.EndArea();

		GUILayout.BeginArea(new Rect(5, Screen.height-205, 300, 200));
		GUILayout.FlexibleSpace();
		GUILayout.Label(logo, GUILayout.Width(200), GUILayout.Height(150));
		GUILayout.EndArea();
	}

	void DrawValue(int x, int y, string key, string value, Color color, bool big) {
		Rect rect = new Rect(x, y, 200, 70);
		if (big) {
			rect.x += 40;
			rect.width -= 80;
			rect.height += 30;
		}

		GUI.Box(rect, string.Empty);

		GUILayout.BeginArea(rect);

		GUILayout.Label(key, "Key");

		GUI.color = color;

		if (big) {
			GUILayout.Label(value, "BigValue");
		} else {
			GUILayout.Label(value, "Value");
		}

		GUI.color = Color.white;

		GUILayout.EndArea();
	}

	bool DrawConfirmButton(string label, bool active) {
		bool clicked = false;

		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal();

		GUILayout.FlexibleSpace();

		if ( active ) {
			GUI.backgroundColor = Color.green;

			if ( GUILayout.Button(label, GUILayout.Width(buttonWidth)) ) {
				clicked = true;
			}
		} else {
			GUI.backgroundColor = SetAlpha(Color.gray, .5f);
			GUI.contentColor = SetAlpha(Color.white, .5f);

			GUILayout.Label(label, "Button", GUILayout.Width(buttonWidth));
		}

		GUI.backgroundColor = Color.white;
		GUI.contentColor = Color.white;

		GUILayout.EndHorizontal();

		GUILayout.Space(2);

		return clicked;
	}

	static public Color SetAlpha(Color color, float alpha) {
    	color.a = alpha;
    	return color;
    }
}

