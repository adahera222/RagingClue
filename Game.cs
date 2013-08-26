using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/*

Game Flow

- Player Creation
  - Enter name
  - Player is given starting funds for wagering

- Choose a category

- Place a wager based on confidence to answer accurately wrt chosen category

- Clues and answer choices are displayed on the clue board

- Player has 10 seconds to correctly answer the question

- Brief pause in between questions for witty banter

============

- If funds reach $0, game over man

- Player earns a star rating for each category?

*/


public enum Category { None,
	ClassicGaming, Movies, RandomAssociation, BadTelevision
}

public enum GameState { None,
	PlayerSetup, Welcome, ChooseCategoryAndWager, ChooseClue, DisplayClue, SubmitAnswer, WittyBanter, GameOver, Credits
}

public enum BanterState { None, 
	TimeExpired, WrongAnswer, RightAnswer, LostGame
}

public class Game : MonoBehaviour {
	public static GameState gameState;
	public static BanterState banterState;

	static string playerNameKey = "playerName";
	public static string playerName = string.Empty;

	public static int startingFunds = 10;
	public static int funds = 0;
	public static int displayFunds = 0;

	public static Category category;
	public static int wager;

	static Clue clue;
	public static Clue clueInstance;

	public static int choice;

    public static float minStars = 0.0f;
    public static float maxStars = 5.0f;
    public static float stars;
    public static string rating;

    public static int maxRounds = 12;
	public static int round;
	public static int timePerRound = 10;
	public static float timeRemaining;

	public static Dictionary<Category, List<Clue>> clues;
	public static List<Category> categories;
	public static Dictionary<Category, string> categoryNames;

	public Light directionalLight;
	public Light spotlight;

	public AudioClip timerSound;
	public AudioClip[] applauseSounds;
	public AudioClip[] failSounds;

	public AudioClip faceshot;
	public AudioClip theme;

	public AudioClip welcomeClip;
	public AudioClip rulesClip;
	public AudioClip[] correctClips;
	public AudioClip[] wrongClips;
	public AudioClip[] expiredClips;
	public AudioClip gameOverClip;
	public AudioClip gameWonClip;
	public AudioClip gameLostClip;
	public AudioClip finalRoundClip;
	AudioClip banterClip;

	static bool playedRulesClip = false;

	public AudioSource creditsAudio;
	public AudioSource loopAudio;
	public AudioSource hostAudio;

	public Fader fader;
	public ScreenOverlay overlay;

	public static Game current;

	static List<string> correctAnswerBanter = new List<string>(){
		"Correctamundo!",
		"Right-o!",
		"Yes, you knew that one, didn't you? You sure did. You sure did. Yep.",
		"You are the fastest googler I've ever met!",
		"You chose... correctly!\n\nWe'll call that one a freebie. Not all of these questions are so easy.",
		"Wow! Correct! You must be some kind of superhuman trivia robot or something! Just kidding, I bet you're all fleshy and squishy inside. Like me.",
		"Someone must have whacked you over the head with the cluebat pretty hard, because you, sir or madam, are 100% correct!",
		"That's a positive hit, buddy! Keep knocking those questions out of the park, and we might not shoot you in the face later.",
		"Are you right, or are you right? You're right! VICTORY CROUCH!",
		"Amazing! Incredible! I hope we have enough Internet Dollars to pay you because we might not!",
		"Hey, look at that. A correct answer! Have some more worthless Internet Dollars!",
		"Exclamation! Interjection! Another correct answer! Ugh.",
		"Yep.\n\nYou're like some kind of living, breathing encyclopedia. Wikipedia is obsolete, because we have you!",
		"I have never seen such correctness in all my life!",
		"If the answer you just gave was wrong, then I don't want to be right! I'm not sure what that means.\n\nCorrect Answer!",
		"You couldn't have been more right!",
		"Hey, you're smart! You can come over to my house later and fix my computer!",
		"I'm going to tweet to my facebooks right now and tell all my friends how smart you are! Just kidding, social media sucks!"
	};

	static List<string> wrongAnswerBanter = new List<string>(){
		"No! Wrong! Bad!",
		"Wrong answer, McFly!",
		"You're so wrong, I'm just going to stand here and laugh at you! Ha! Ha! Wrong!",
		"You are so very, very wrong. So much wrongness.",
		"Wrong! Wrong! Wrong! No!\n\nI love my job.",
		"You're wrong, and really bad at this!\n\nI'm sorry, that was mean.",
		"Nope. Nope. Nope. Hope you didn't need those Internet Dollars, because we're taking them back.",
		"Keep answering wrong. No, really. There's a special prize at the end for losers.",
		"My wrong-o-meter is spiking hard! Because of your wrong answer!",
		"You, sir or madame, are wrong!",
		"Every time you answer wrong, I feel a little bit better about myself! It's a personality flaw!",
		"Wrongola! Incorrecto! Wrong-answer-o!",
		"That was not the answer we were looking for!",
		"I checked the big book of people who were wrong just now, and you were in it.",
		"You couldn't be wronger!",
		"Worst answer ever!",
		"Here's a pro tip, stop being wrong!",
		"Wrong answer, Wrongy McWrongington!"
	};

	static List<string> timeExpiredBanter = new List<string>(){
		"Out of time!",
		"And you're too slow!",
		"No answer? Wrong answer!",
		"Hello? Anybody home? You ran out of time on that one.",
		"You took too long!",
		"I want the last ten seconds of my life back!",
		"Soooo sloooooowwwwwwww. Someone's been dipping into the doggie downers.",
		"You failed to answer! That's like answering wrong!",
		"You. Took. Too. Long. Gentle reminder here -- you've only got ten seconds to answer.",
		"It's better to make a wild guess than to sit there like a lump.",
		"Oh no you didn't! No, really, you didn't. Time expired, sorry.",
		"Waiting for you to answer is like watching a Scandinavian movie. Hurry up, would you?",
		"Time!",
		"Oh, I'm sorry, is this game distracting you from your busy schedule?",
		"Your silence is deafening!"
	};

	public static string banter;
	
	void Awake() {
		/* if (Application.isWebPlayer) {
			if (string.Compare(Application.absoluteURL, "http://zigfrak.com/ludum27/RagingClue.unity3d", true) != 0)
				this.enabled = false;
				return;
		} */

		current = this;

		SetGameState(GameState.PlayerSetup);
	}

	void SetupCategories() {
		clues = new Dictionary<Category, List<Clue>>();
		categories = new List<Category>();
		categoryNames = new Dictionary<Category, string>();

		foreach ( Category c in System.Enum.GetValues(typeof(Category)) ) {
    		foreach ( Clue clue in Resources.LoadAll(c.ToString(), typeof(Clue)) ) {
    			if ( !clues.ContainsKey(c) ) clues.Add(c, new List<Clue>());
    			clues[c].Add(clue);
    		}

    		if ( clues.ContainsKey(c) ) {
    			categories.Add(c);
    			categoryNames[c] = Regex.Replace(c.ToString(),"(\\B[A-Z])"," $1");
    		}
		}
	}

	void Update() {
		if ( gameState != GameState.Credits ) {
			FadeOutCreditsAudio();
		}

		switch(gameState) {
			case GameState.PlayerSetup:
				FadeInLoop();
				FadeInSpotlight();
				FadeOutDirectionalLight();
				break;

			case GameState.ChooseClue:
				FadeInLights();
				ChooseClue();
				break;

			case GameState.DisplayClue:
				FadeOutLoop();
				DimLights();
				Countdown();
				break;

			case GameState.SubmitAnswer:
				FadeInLights();
				HandleAnswer();
				break;

			case GameState.GameOver:
				FadeOutLoop();
				FadeInSpotlight();
				FadeOutDirectionalLight();
				break;

			case GameState.ChooseCategoryAndWager:
				FadeOutLoop();
				FadeInLights();
				break;

			case GameState.Credits:
				FadeOutLoop();
				FadeInLights();
				break;

			default:
				FadeInLoop();
				FadeInLights();
				break;
		}
	}

	void FadeInLoop() {
		loopAudio.volume = Mathf.Lerp(loopAudio.volume, 0.4f, Time.deltaTime);
	}

	void FadeOutLoop() {
		loopAudio.volume = Mathf.Lerp(loopAudio.volume, 0.0f, Time.deltaTime);
	}

	void PlayCreditsAudio() {
		current.creditsAudio.Stop();
		creditsAudio.volume = 1.0f;
		current.creditsAudio.PlayOneShot(current.theme);
	}

	void FadeOutCreditsAudio() {
		creditsAudio.volume = Mathf.Lerp(creditsAudio.volume, 0.0f, Time.deltaTime);
	}

	void FadeInLights() {
		directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, .5f, Time.deltaTime);

		FadeInSpotlight();
	}

    void FadeOutDirectionalLight() {
		directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, 0f, Time.deltaTime);
    }

	void FadeInSpotlight() {
		spotlight.intensity = Mathf.Lerp(spotlight.intensity, 1.5f, Time.deltaTime);
	}

	void DimLights() {
		directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, .125f, Time.deltaTime);

		spotlight.intensity = Mathf.Lerp(spotlight.intensity, 1.0f, Time.deltaTime);
	}

	void ChooseClue() {
		clue = clues[category][Random.Range(0, clues[category].Count)];
		clues[category].Remove(clue);

		if ( clues[category].Count == 0 ) {
			clues.Remove(category);
			categories.Remove(category);
		}

		ScrambleClue();

		timeRemaining = timePerRound;

		SetGameState(GameState.DisplayClue);
	}

	void Countdown() {
		timeRemaining -= Time.deltaTime;

		if ( timeRemaining <= 0.0f ) {
			banterState = BanterState.TimeExpired;
			int i = Random.Range(0, timeExpiredBanter.Count);
			banter = timeExpiredBanter[i];
			banterClip = expiredClips[i];
			HandleFailure();
		}
	}

	void HandleAnswer() {
		int i;
		if ( choice == clueInstance.correctChoice ) {
			banterState = BanterState.RightAnswer;
			i = Random.Range(0, correctAnswerBanter.Count);
			banter = correctAnswerBanter[i];
			banterClip = correctClips[i];
			HandleSuccess();
		} else {
			banterState = BanterState.WrongAnswer;
			i = Random.Range(0, wrongAnswerBanter.Count);
			banter = wrongAnswerBanter[i];
			banterClip = wrongClips[i];
			HandleFailure();
		}
	}

	void HandleSuccess() {
		stars = Mathf.Clamp(stars, 1.0f, maxStars);

		if ( timeRemaining > 5.0f ) {
			stars = Mathf.Clamp(stars + .2f, minStars, maxStars);
		} else if ( timeRemaining > 2.5f ) {
			stars = Mathf.Clamp(stars + .1f, minStars, maxStars);
		} else {
			stars = Mathf.Clamp(stars + .05f, minStars, maxStars);
		}

		if ( wager == funds ) {
			stars = Mathf.Clamp(stars + .4f, minStars, maxStars);
		} else if ( wager > funds * .5f ) {
			stars = Mathf.Clamp(stars + .2f, minStars, maxStars);
		}

		UpdateStars();

		funds += wager;

		SetGameState(GameState.WittyBanter);
		current.audio.Stop();
		current.audio.PlayOneShot(current.applauseSounds[Random.Range(0, current.applauseSounds.Length)]);

		current.hostAudio.PlayOneShot(banterClip);
	}

	void HandleFailure() {
		funds -= wager;

		stars = Mathf.Clamp(stars - .5f, minStars, maxStars);

		if ( wager == funds ) {
			stars = Mathf.Clamp(stars - .5f, minStars, maxStars);
		} else if ( wager > funds * .5f ) {
			stars = Mathf.Clamp(stars - .25f, minStars, maxStars);
		}

		UpdateStars();
		
		if ( funds <= 0 ) {
			SetGameState(GameState.GameOver);
			return;
		} else {
			SetGameState(GameState.WittyBanter);
		}

		if ( banterState != BanterState.TimeExpired ) {
			current.audio.Stop();
		}
		current.audio.PlayOneShot(current.failSounds[Random.Range(0, current.failSounds.Length)]);

		current.hostAudio.PlayOneShot(banterClip);
	}

	public static void SetGameState(GameState newGameState) {
		gameState = newGameState;

		switch(gameState) {
			case GameState.PlayerSetup:
				current.hostAudio.Stop();
				current.hostAudio.PlayOneShot(current.welcomeClip);
				SetupGame();
				break;

			case GameState.Welcome:
				if ( !playedRulesClip ) {
					current.hostAudio.Stop();
					current.hostAudio.PlayOneShot(current.rulesClip);
					playedRulesClip = true;
				}
				EasePosition.current.Change();
				PlayerPrefs.SetString(playerNameKey, playerName);
				break;

			case GameState.DisplayClue:
				current.audio.PlayOneShot(current.timerSound);
				break;

			case GameState.ChooseCategoryAndWager:
				current.hostAudio.Stop();

				EasePosition.current.Change();

				category = categories[Random.Range(0, categories.Count)];

				round++;

				if ( round == maxRounds ) {
					current.hostAudio.PlayOneShot(current.finalRoundClip);
				}

				if ( round > maxRounds ) {
					round = maxRounds;
					SetGameState(GameState.GameOver);
				}

				break;

			case GameState.ChooseClue:
				break;

			case GameState.SubmitAnswer:
				break;

			case GameState.GameOver:
				current.HandleGameOver();
				break;

			case GameState.Credits:
				current.PlayCreditsAudio();
				current.audio.PlayOneShot(current.applauseSounds[0]);
				current.audio.PlayOneShot(current.applauseSounds[1]);
				current.audio.PlayOneShot(current.applauseSounds[2]);
				break;

			default:
				EasePosition.current.Change();
				break;
		}
	}

	static void SetupGame() {
		current.SetupCategories();

		current.fader.color = Color.black;
		current.fader.alpha = 1.0f;

		current.overlay.enabled = false;

		current.audio.PlayOneShot(current.applauseSounds[0]);
		current.audio.PlayOneShot(current.applauseSounds[1]);
		current.audio.PlayOneShot(current.applauseSounds[2]);

		banterState = BanterState.None;

		if ( PlayerPrefs.HasKey(playerNameKey) ) {
			playerName = PlayerPrefs.GetString(playerNameKey);
		} else {
			playerName = string.Empty;
		}

		funds = startingFunds;
		displayFunds = 0;
		round = 0;

		stars = minStars;
		UpdateStars();
	}

	static void UpdateStars() {
		rating = string.Empty;
		for ( int i = 1; i <= stars; i++ ) {
			rating += "★";
		}
	}

	void HandleGameOver() {
		if ( funds == 0 ) {
			current.audio.Stop();
			audio.PlayOneShot(faceshot);

			banterState = BanterState.LostGame;

			fader.color = Color.white;
			fader.alpha = 1.0f;

			overlay.enabled = true;

			current.hostAudio.Stop();
			current.hostAudio.PlayOneShot(current.gameLostClip);
		} else {
			current.audio.PlayOneShot(current.applauseSounds[0]);
			current.audio.PlayOneShot(current.applauseSounds[1]);
			current.audio.PlayOneShot(current.applauseSounds[2]);

			current.hostAudio.Stop();
			current.hostAudio.PlayOneShot(current.gameWonClip);
		}
	}

	void ScrambleClue() {
		if ( clueInstance ) Destroy(clueInstance.gameObject);
		clueInstance = Instantiate(clue) as Clue;

		int t;
		string swap;
		for ( int i = 0; i < clueInstance.clues.Length; i++ ) {
			t = Random.Range(0, clueInstance.clues.Length);
			swap = clueInstance.clues[i];

			clueInstance.clues[i] = clueInstance.clues[t];
			clueInstance.clues[t] = swap;
		}

		for ( int i = 0; i < clueInstance.choices.Length; i++ ) {
			t = Random.Range(0, clueInstance.choices.Length);
			swap = clueInstance.choices[i];

			clueInstance.choices[i] = clueInstance.choices[t];
			clueInstance.choices[t] = swap;

			if ( clueInstance.correctChoice == i ) {
				clueInstance.correctChoice = t;
			} else if ( clueInstance.correctChoice == t ) {
				clueInstance.correctChoice = i;
			}
		}

	}
}
