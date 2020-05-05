using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public enum E_CardCombinationType {		
	[Description("Undefine")]					Undefine = -1 ,
	[Description("Rayal Straigth Flush")]	RoyalStraigthFlush = 10 , 
	[Description("Straigth Flush")]			StraigthFlush = 9 , 
	[Description("Four of a Kind")]			FourOfaKind = 8 , 
	[Description("Full House")]				FullHouse = 7 , 
	[Description("Flush")]						Flush = 6 , 
	[Description("Straight")]					Straight = 5 , 
	[Description("Three of a Kind")]		ThreeOfAKind = 4 , 
	[Description("Two Pair")]					TwoPair = 3 , 
	[Description("One Pair")]					OnePair = 2 , 
	[Description("High Card")]				HighCard = 1 , 
	[Description(" - count - ")]	Count = 99 
}

public class CardCombinationData {

	const float weightFactor = 16;
	float divider1 = Mathf.Pow (weightFactor, 1f);
	float divider2 = Mathf.Pow (weightFactor, 2f);
	float divider3 = Mathf.Pow (weightFactor, 3f);
	float divider4 = Mathf.Pow (weightFactor, 4f);

	public E_CardCombinationType type = E_CardCombinationType.Undefine ;
	public float [] cardSetValueArray = new float[5];

	public void SetData ( E_CardCombinationType _type , CCardData [] cards ) {
		type = _type;
		for ( int i = 0 ; i < cardSetValueArray.Length ; ++i )
			cardSetValueArray [i] = (float)cards [i].Value ;
	}

	// total weight = type weight + value weight
	public float GetWeight () {
		return (int)type * weightFactor + GetValueWeight ();
	}

	// important algorithms
	float GetValueWeight () {
		switch (type) {
		case E_CardCombinationType.RoyalStraigthFlush:
		case E_CardCombinationType.StraigthFlush:
		case E_CardCombinationType.Straight:
			// lead card value decide card weight
			return cardSetValueArray [0] ;
		case E_CardCombinationType.FourOfaKind:
			// fifth card value decide card weight
			return cardSetValueArray [4] ;
		case E_CardCombinationType.FullHouse:
			return cardSetValueArray [0] + cardSetValueArray [3]/divider1 ;
		case E_CardCombinationType.ThreeOfAKind:
			return cardSetValueArray [0] + cardSetValueArray [3]/divider1 + cardSetValueArray [4]/divider2 ;
		case E_CardCombinationType.TwoPair:
			return cardSetValueArray [0] + cardSetValueArray [2]/divider1 + cardSetValueArray [4]/divider2 ;
		case E_CardCombinationType.OnePair:
			return cardSetValueArray [0] + cardSetValueArray [2]/divider1 + cardSetValueArray [3]/divider2 + cardSetValueArray [4]/divider3 ;
		case E_CardCombinationType.Flush:
		case E_CardCombinationType.HighCard:
			return cardSetValueArray [0] + cardSetValueArray [1]/divider1 + cardSetValueArray [2]/divider2 + cardSetValueArray [3]/divider3 + cardSetValueArray [4]/divider4 ;
		default:
			return 0;
		}
	}

}
public class CPlayer : MonoBehaviour {

	const int ACE_value = 14;
	public List<CCardData> HandCards ;
	public List<CCardData> CurrentBestCardList ;
	public CardCombinationData CurrentBestCardCombination;

	public Text TextBestCardForm ;

	// Use this for initialization
	void Start () {

	}

	public void DrawCards () {

	}

	public float GetBestCardWeight () {
		return CurrentBestCardCombination.GetWeight ();
	}

	int CardComparer ( CCardData card1 , CCardData card2 ) {
		if (card1.GetWeight () < card2.GetWeight ())
			return 1;
		if (card1.GetWeight () == card2.GetWeight ())
			return 0;
		if (card1.GetWeight () > card2.GetWeight ())
			return -1;
		return -1;
	}

	E_CardCombinationType BestPatternMatch ( CCardData [] sources ) {
		return E_CardCombinationType.Undefine;
	}

	List<CCardData> SortList ( bool showDebugLog , List<CCardData> sources ) {
		
		if (showDebugLog) {
			Debug.Log (" --- before sort --- ");
			foreach (CCardData data in sources)
				Debug.Log ("value : " + data.Value + " , weight : " + data.GetWeight ());
		}

		// sort card list by weight
		sources.Sort (CardComparer);

		if (showDebugLog) {
			Debug.Log (" --- after sort --- ");
			foreach (CCardData data in sources)
				Debug.Log ("value : " + data.Value + " , weight : " + data.GetWeight ());
			Debug.Log (" --- end sort --- ");
		}

		return sources;
	}

	// 7 pick 5 , Main Algorithm
	public CardCombinationData GetBestCardCombination ( CCardData [] sources ) {

		CardCombinationData result = new CardCombinationData () ;
		List<CCardData> newSet = new List<CCardData> ();

		foreach (CCardData data in sources)
			newSet.Add (data);

		foreach ( CCardData data in HandCards )
			newSet.Add (data);		

//		newSet.Sort (CardComparer);
		newSet = SortList ( false , newSet ) ;

		// run best pattern match algorithm
		int pairCount = 0;
		bool isFlush = false;
		bool isStraight = false;
		bool isFourOfaKind = false;

		bool isAceLead = false;
		bool hasThreeOfaKind = false;

		// Check first card is ACE or not
		isAceLead = newSet [ 0 ].Value == ACE_value ;

		// Straight check 
		int maxContinuousCount = 0;
		int continuousCounter = 0;
		int startIndex_continuous = 0;
		bool hasStartIndex_continuous = false;

		int maxEqualCount = 0;
		int equalCounter = 0;
		int startIndex_equal = 0;
		bool hasStartIndex_equal = false;

		int [] subValueArray = new int [ newSet.Count - 1 ] ;

		for (int i = 0; i < newSet.Count - 1; ++i) {
			
			subValueArray [i] = newSet [i].Value - newSet [i + 1].Value;

			if (subValueArray [i] == 1) {
				
				continuousCounter++;

				// save max counter
				if ( continuousCounter > maxContinuousCount )
					maxContinuousCount = continuousCounter;

				if (hasStartIndex_continuous == false) {
					startIndex_continuous = i;
					hasStartIndex_continuous = true;
				}
			} else {

				// save max counter
				if ( continuousCounter > maxContinuousCount )
					maxContinuousCount = continuousCounter;

				continuousCounter = 0;
				hasStartIndex_continuous = false ;
			}

			// check max the same card count
			if (subValueArray [i] > 0) {
				
				// save max counter
				if (equalCounter > maxEqualCount) {
					maxEqualCount = equalCounter;
					hasStartIndex_equal = false;
				}
				
				equalCounter = 0;

			} else {
				// subValue == 0 ( no conditions in < 0 after sort )

				equalCounter++;

				if (equalCounter > maxEqualCount) {
					maxEqualCount = equalCounter;
					hasStartIndex_equal = false;
				}

				if (hasStartIndex_equal == false) {
					startIndex_equal = i;
					hasStartIndex_equal = true;
				}
			}

		}


		if (maxContinuousCount > 4)
			isStraight = true;

		// Four of a kind check
		if (maxEqualCount == 3)
			isFourOfaKind = true;

		// Three of a kind check
		if (maxEqualCount == 2)
			hasThreeOfaKind = true;

		// is pair pattern
		int startIndex_pair1 = -1;
		int startIndex_pair2 = -1;
		if (maxEqualCount == 1) {
			
			// three pair
			// x 0 x 0 x 0 
			// 0 x 0 x 0 x 

			// two pair
			// 0 x 0 x x x
			// 0 x x 0 x x
			// 0 x x x 0 x
			// 0 x x x x 0

			// x 0 x x x 0
			// x 0 x x 0 x 
			// x 0 x 0 x x 

			// x x 0 x x 0
			// x x 0 x 0 x

			// x x x 0 x 0

			// one pair
			// 0 x x x x x
			// x 0 x x x x
			// x x 0 x x x
			// x x x 0 x x
			// x x x x 0 x
			// x x x x x 0

			// check pair count
			for (int i = 0; i < subValueArray.Length; i++) {
				if (subValueArray [i] == 0) {
					
					pairCount++;

					if (pairCount == 1)
						startIndex_pair1 = i;
					
					if (pairCount == 2)
						startIndex_pair2 = i;
					
				}
			}
			
		}

		// Flush Check
		int spadeCount = 0;
		int heartCount = 0;
		int diamondCount = 0;
		int cludCount = 0;

		CardType flushType = CardType.Undefine;

		foreach (CCardData card in newSet) {
			if (card.Type == CardType.Spade)
				spadeCount++;
			if (card.Type == CardType.Heart)
				heartCount++;
			if (card.Type == CardType.Diamond)
				diamondCount++;
			if (card.Type == CardType.Club)
				cludCount++;
		}

		if (spadeCount >= 5)
			flushType = CardType.Spade;
		if (heartCount >= 5)
			flushType = CardType.Heart;
		if (diamondCount >= 5)
			flushType = CardType.Diamond;
		if (cludCount >= 5)
			flushType = CardType.Club;

		if ( flushType != CardType.Undefine )
			isFlush = true;

		// -------------------------------------------------------------------------------------


		// otherwise is High Card (Zitch)
		result.type = E_CardCombinationType.HighCard ;

		// one pair check
		if (pairCount == 1)
			result.type = E_CardCombinationType.OnePair;
		
		// two pair check
		if (pairCount == 2)
			result.type = E_CardCombinationType.TwoPair;

		// three of kind check
		if (hasThreeOfaKind)
			result.type = E_CardCombinationType.ThreeOfAKind;
		
		// Straight check
		if (isStraight)
			result.type = E_CardCombinationType.Straight;

		// Flush check
		if (isFlush)
			result.type = E_CardCombinationType.Flush;

		// Full house check
		if (hasThreeOfaKind && pairCount >= 1)
			result.type = E_CardCombinationType.FullHouse;

		// Four of a kind check
		if (isFourOfaKind)
			result.type = E_CardCombinationType.FourOfaKind;
		
		if (isStraight && isFlush) {
			// Royal straigth flush check
			if ( isAceLead )
				result.type = E_CardCombinationType.RoyalStraigthFlush;
			else
				// straigth flush check
				result.type = E_CardCombinationType.StraigthFlush;
		}
		
		// pick card
		const int MaxHandCardCount  = 5 ;
		CCardData[] pickedCardValue = new CCardData [ MaxHandCardCount ] ;

		List<CCardData> pickedCardList = new List<CCardData> ();
		Queue<CCardData> valueSetQueue = new Queue<CCardData>() ;

		switch (result.type) {
		case E_CardCombinationType.RoyalStraigthFlush:
		case E_CardCombinationType.StraigthFlush:
		case E_CardCombinationType.Straight:
			for ( int i = startIndex_continuous ; i < startIndex_continuous + MaxHandCardCount ; ++i )
				pickedCardValue [ i - startIndex_continuous ] = newSet [i];
			break;
		case E_CardCombinationType.FourOfaKind:
			if (startIndex_equal == 0) {
				// A A A A K Q J
				for (int i = 0; i < MaxHandCardCount ; ++i)
					pickedCardValue [i] = newSet [i];
			} else {
				// A 9 8 8 8 8 7
				for ( int i = startIndex_equal , j = 0 ; i < MaxHandCardCount - 1 ; ++i , ++ j  )
					pickedCardValue [j] = newSet [i];
				// set max value card into fifth card
				pickedCardValue [4] = newSet [0];
			}
			break;
		case E_CardCombinationType.FullHouse:
			if ( startIndex_equal == 0) {
				for (int i = 0; i < pickedCardValue.Length; ++i)
					pickedCardValue [i] = newSet [i];
			} else {
				// A KK Q JJJ
				for (int i = startIndex_equal , j = 0 ; i < startIndex_equal + 3 ; ++i  , ++ j )
					pickedCardValue [i] = newSet [i];
				pickedCardValue [3] = newSet [ startIndex_pair1 ]  ;
				pickedCardValue [4] = newSet [ startIndex_pair1 + 1 ] ;
			}
			break;
		case E_CardCombinationType.Flush:
			int pickedCardCount = 0;
			foreach (CCardData card in newSet) {
				if ( pickedCardCount < MaxHandCardCount )
					if (card.Type == flushType) {
						pickedCardValue [ pickedCardCount ] = card;
						pickedCardCount++;
					}
			}
			break;
		case E_CardCombinationType.ThreeOfAKind:
			// A K Q J 9 9 9
			// A K K K Q J 9
			pickedCardList.Add (newSet [startIndex_equal]);
			pickedCardList.Add (newSet [startIndex_equal + 1]);
			pickedCardList.Add (newSet [startIndex_equal + 2 ]);

			foreach (CCardData card in newSet)
				if (card.Value != newSet [startIndex_equal].Value)
					valueSetQueue.Enqueue (card);

			pickedCardList.Add (valueSetQueue.Dequeue ());
			pickedCardList.Add (valueSetQueue.Dequeue ());

			break;
		case E_CardCombinationType.TwoPair:

			pickedCardList.Add (newSet [startIndex_pair1]);
			pickedCardList.Add (newSet [startIndex_pair1 + 1]);
			pickedCardList.Add (newSet [startIndex_pair2]);
			pickedCardList.Add (newSet [startIndex_pair2 + 1]);

			foreach (CCardData card in newSet)
				if (card.Value != newSet [startIndex_pair1].Value || card.Value != newSet [startIndex_pair2].Value)
					valueSetQueue.Enqueue (card);
			
			pickedCardList.Add (valueSetQueue.Dequeue ());

			break;
		case E_CardCombinationType.OnePair:

			pickedCardList.Add (newSet [startIndex_pair1]);
			pickedCardList.Add (newSet [startIndex_pair1 + 1]);

			foreach (CCardData card in newSet)
				if (card.Value != newSet [startIndex_pair1].Value)
					valueSetQueue.Enqueue (card);
			
			pickedCardList.Add (valueSetQueue.Dequeue ());
			pickedCardList.Add (valueSetQueue.Dequeue ());
			pickedCardList.Add (valueSetQueue.Dequeue ());

			break;
		case E_CardCombinationType.HighCard:
			for ( int i = 0 ; i < MaxHandCardCount ; i ++ )
				pickedCardList.Add ( newSet [i] );
			break;
		default:
			break;
		}

		if (pickedCardList.Count > 0)
			pickedCardValue = pickedCardList.ToArray ();

		result.SetData ( result.type , pickedCardValue ) ;

		for ( int i = 0 ; i < MaxHandCardCount ; i ++ )
			CurrentBestCardList[i].SetData (pickedCardValue [i]);

		CurrentBestCardCombination = result ;
		TextBestCardForm.text = result.type.GetDescription ();

		return result ;

	}

	// Update is called once per frame
	void Update () {

	}

}