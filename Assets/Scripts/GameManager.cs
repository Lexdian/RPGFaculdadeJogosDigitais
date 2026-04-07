using UnityEngine;

public class GameManager : MonoBehaviour
{
    public RuntimeAnimatorController[] animators = new RuntimeAnimatorController[4];
    public GameObject lider;
    public GameObject followers;
    void Start()
    {
        CreateTeam();
    }

    private void CreateTeam()
    {
        GameObject liderChar = GameObject.Instantiate(lider, new Vector2(0,0), Quaternion.identity);
        liderChar.GetComponent<LiderCharacter>().Setup(animators[0]);
        for (int i = 1; i < animators.Length; i++)
        {
            if(animators[i] == null)
            {
                continue;
            }
            GameObject newChar = GameObject.Instantiate(followers, new Vector2(0, 0), Quaternion.identity);
            newChar.GetComponent<Character>().Setup(animators[i]);
            liderChar.GetComponent<LiderCharacter>().followers[i-1] = newChar.GetComponent<Character>();
        }
    }
}
