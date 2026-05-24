open System
open System.Threading

type Element =
    | Fire
    | Water
    | Grass
    | Prism
    member this.ToIcon() =
        match this with | Fire -> "🔥" | Water -> "💧" | Grass -> "🍃" | Prism -> "💎"
    override this.ToString() =
        match this with | Fire -> "fire" | Water -> "water" | Grass -> "grass" | Prism -> "prism"

type Player = {
    mutable Hp: int; mutable MaxHp: int;
    mutable Reels: Element list list 
    mutable FireDamage: float; mutable WaterDamage: float; mutable GrassDamage: float ; mutable PrismDamage:float
}

type Enemy = {
    Name: string; mutable Hp: int; MaxHp: int; ElementType: Element ; Damage : int;
}

type GameState = {
    Player: Player; CurrentEnemy: Enemy; Round: int; RoundElement: Element;
}


module GameLogic =
    let spinSlot (playerReels: Element list list)  =
        let rand = Random()
        let spinSingleReel (reel: Element list) =
            let extendedReel= reel @ reel
            let randomIndex = rand.Next(5)
            extendedReel |> List.skip randomIndex |> List.take 3

        playerReels |> List.map spinSingleReel

    let createEnemy (round:int) (elementtype: Element) = 
        if round % 3 = 0 then
            let hp = 50 + 4 * (round - 1)
            let damage = 15 + round * 2 - 2
            {Name = elementtype.ToString() + " Boss"; Hp = hp; MaxHp = hp; ElementType = elementtype; Damage = damage }
        elif round = 10 then
            let hp = 200
            let damage = 30
            {Name =  "Final Boss"; Hp = hp; MaxHp = hp; ElementType = elementtype; Damage = damage }
        else
            let hp = 30 + 4 * (round - 1)
            let damage = 8 + int(float(round) * 1.3) - 2
            {Name = elementtype.ToString() + " Monster"; Hp = hp; MaxHp = hp; ElementType = elementtype; Damage = damage }

    let findInReels counts symbol =
            counts |> List.tryFind( fun (sym, _) -> sym = symbol)
            |> function | Some(_, count) -> count | None -> 0

    let countElementInReels  (resultReels: Element list list) = 
        let reelsCount = resultReels |> List.concat |> List.countBy id
        reelsCount
    
    let calcuDamage player  enemy (resultReels: Element list list) =
        let reelsCount = countElementInReels resultReels
        let weight = 
            match enemy.ElementType with
            | Fire -> [1.0; 2.0; 0.5; 1.5]
            | Water -> [0.5; 1.0; 2.0; 1.5]
            | Grass -> [2.0; 0.5; 1.0; 1.5]
            | Prism -> [1.0; 1.0; 1.0; 1.5]
        let damageList = [player.FireDamage *  float( findInReels reelsCount Fire )* weight.[0];
        player.WaterDamage * float (findInReels reelsCount Water )* weight.[1] ;
        player.GrassDamage * float( findInReels reelsCount Grass )* weight.[2];
        player.PrismDamage *  float(findInReels reelsCount Prism )* weight.[3]; ] |> List.sortDescending
        damageList.[0] + damageList.[1]
        

    let changeRandomElements (reels: Element list list) cnt (types: option<Element>) prismbool =
        let rand = Random()
        let flatReels = List.collect id reels

        // TODO: round.Player.Reels 안에 cnt개 이상의 types가 들어있는지 체크하고, 그럴 경우 랜덤으로


        let targetIndexs = 
            match types with 
            | Some(t) ->
                flatReels
                |> List.indexed 
                |> List.filter (fun (_, e) -> e = t )
                |> List.map fst
            | None ->
                [0 .. 24] 
        
        let choosetype current =  
            let availableTypes = 
                if prismbool then [Water; Fire; Grass; Prism]
                else [Water; Fire; Grass]
            
            let filteredTypes = availableTypes |> List.filter (fun t -> t <> current)
            filteredTypes.[rand.Next(filteredTypes.Length)]
        
        let chosenIndexs =  // 바꿀 위치
            if targetIndexs.Length >= cnt then targetIndexs |> List.sortBy (fun _ -> rand.Next()) |> List.truncate cnt 
            else 
                let needed = cnt - targetIndexs.Length
                let otherIndexs = 
                    [0 .. (flatReels.Length - 1)]
                    |> List.filter (fun i -> not (List.contains i targetIndexs))
                    |> List.sortBy (fun _ -> rand.Next())
                    |> List.truncate needed
                targetIndexs @ otherIndexs


        let updatedFlat = 
            flatReels |> List.mapi (fun i e -> 
                if List.contains i chosenIndexs then
                    let newType = choosetype e
                    printfn "Symbol at index %d transformed: %A → %A" i e newType 
                    newType
                else e)
        
        updatedFlat |> List.chunkBySize 5
    


    let getReward (round: GameState) =
        let rand = Random()
        let reward1type = 
            match round.RoundElement with 
            | Fire -> Grass
            | Water -> Fire
            | Grass -> Water
            | _ -> [Grass;Fire;Water].[rand.Next(3)]                

        let reward2type = [Grass;Fire;Water;Prism].[rand.Next(4)]
        let reward2percent =  [1.2 ; 1.3; 1.4].[rand.Next(3)]

        if round.Round % 3 = 0 then     // 보스용 
            round.Player.Hp <- round.Player.Hp + int(float( round.Player.MaxHp -  round.Player.Hp ) * 0.7)
            printfn "Recovers 70%% of lost Hp : your Hp is now %d" round.Player.Hp 
            printfn "1. Randomly changes five random symbols into different attributes (including Prism)"
            printfn "2. Increases the damage of every attribute by %f times"  reward2percent
            printfn "3. increase MaxHP and HP for 40"
            
        else 
            printfn "1. Randomly changes three %s into different attributes (excluding Prism)" (reward1type.ToString())
            printfn "2. Increases the damage of a %s attribute by %f times" (reward2type.ToString()) reward2percent
            printfn "3. Randomly changes three random symbols into different attributes (including Prism)"

        let rec getValidInput () =
            printf "Choose one option: "
            let input = Console.ReadLine()

            match input with
            | "1" -> 
                if round.Round % 3 = 0 then
                    round.Player.Reels <- changeRandomElements round.Player.Reels 5 None true
                else
                    round.Player.Reels <- changeRandomElements round.Player.Reels 3 (Some reward1type) false
            | "2" -> 
                if round.Round % 3 = 0 then
                    round.Player.GrassDamage <- int (Math.Ceiling (float round.Player.GrassDamage * reward2percent))
                    round.Player.FireDamage <- int (Math.Ceiling (float round.Player.FireDamage * reward2percent))
                    round.Player.WaterDamage <- int (Math.Ceiling (float round.Player.WaterDamage * reward2percent))
                    round.Player.PrismDamage <- int (Math.Ceiling (float round.Player.PrismDamage * reward2percent))
                else
                    match reward2type with
                    | Grass -> round.Player.GrassDamage <- int (Math.Ceiling (float round.Player.GrassDamage * reward2percent))
                    | Fire -> round.Player.FireDamage <- int (Math.Ceiling (float round.Player.FireDamage * reward2percent))
                    | Water -> round.Player.WaterDamage <- int (Math.Ceiling (float round.Player.WaterDamage * reward2percent))
                    | Prism -> round.Player.PrismDamage <- int (Math.Ceiling (float round.Player.PrismDamage * reward2percent))
                    printfn "%s damage increased to %f!" (reward2type.ToString()) reward2percent
            | "3" -> 
                if round.Round % 3 = 0 then
                    round.Player.MaxHp <- round.Player.MaxHp + 40
                    round.Player.Hp <- round.Player.Hp + 40
                else
                    round.Player.Reels <- changeRandomElements round.Player.Reels 3 None true
            | _ -> 
                printfn "Please enter only the values from the options provided."
                getValidInput () 

        getValidInput ()
        Thread.Sleep 300
            



    let gameOver round =
        printf " %d round cleared. Thank you for playing."  round
        // 플레이어에 대한 정보 더 줄까? 필요없을듯


    


    
    
module Renderer =
    let renderSlotSpinning (playerReels: Element list list) =
        Console.Clear()
        printfn "\n----- SLOT SPINNING -----"
        let rand = Random()
        let finalResult = GameLogic.spinSlot playerReels

        let totalFrames = 20
        
        for frame in 0 .. totalFrames do
            let sleepTime = 30 + frame * 5
            Thread.Sleep sleepTime
            
            Console.SetCursorPosition(0, 2)
            printfn "┌────┬────┬────┬────┬────┐"
            for r in 0 .. 2 do
                printf "│"
                for c in 0 .. 4 do
                    let stopFrame = (c + 1) * 4
                    
                    if frame >= stopFrame then
                        printf " %s │" (finalResult.[c].[r].ToIcon())
                    else
                        let randomReelIndex = rand.Next(playerReels.[c].Length)
                        printf " %s │" (playerReels.[c].[randomReelIndex].ToIcon())
                printfn ""
            printfn "└────┴────┴────┴────┴────┘"
        
        Thread.Sleep 300
        Console.SetCursorPosition(0, 1)
        printfn "\n--- SLOT RESULT ---             "
        Console.SetCursorPosition(0, 2)
        printfn "┌────┬────┬────┬────┬────┐"
        for r in 0 .. 2 do
            printf "│"
            for c in 0 .. 4 do
                printf " %s │" (finalResult.[c].[r].ToIcon())
            printfn ""
        printfn "└────┴────┴────┴────┴────┘"
        finalResult
    
    let printState (state: GameState) =
        let enemy = state.CurrentEnemy
        printfn "\n=========================================="
        printfn "round: %d | enemy: %s [%A]" state.Round enemy.Name enemy.ElementType
        printfn "enemy HP: %d/%d" enemy.Hp enemy.MaxHp
        printfn "player HP: %d/%d" state.Player.Hp state.Player.MaxHp
        printfn "------------------------------------------"
        printfn "Press Enter to spin the slot..."

    let printinitial (reels: Element list list) =
        Console.Clear()
        
        printfn "=========================================="
        printfn "           Game Start!             "
        printfn "       Initial Reel Settings:             "
        printfn "=========================================="
        printfn "┌────┬────┬────┬────┬────┐"
        for r in 0 .. 4 do
            printf "│"
            for c in 0 .. 4 do
                printf " %s │" (reels.[c].[r].ToIcon())
            printfn ""
        printfn "└────┴────┴────┴────┴────┘"


    


[<EntryPoint>]
let main argv =
    Console.OutputEncoding <- System.Text.Encoding.UTF8 

    // 기본 세팅    
    let rand= Random()
    let initialFlatList =  (List.replicate 8 Water) @ (List.replicate 8 Fire) @ (List.replicate 8 Grass) @  [Prism]|> List.sortBy (fun _ -> rand.Next())
    let initialReels = initialFlatList |> List.chunkBySize 5


    let initialPlayer = {
        Hp = 120; MaxHp = 120;
        Reels = initialReels;
        FireDamage = 1; WaterDamage = 1; GrassDamage = 1; PrismDamage = 1
    }

    let initialState = {
        Player = initialPlayer
        CurrentEnemy =  GameLogic.createEnemy 1 Fire 
        Round = 1
        RoundElement = Fire
    }

    let rec roundPlay gamestate  =
        Renderer.printState gamestate
        Console.ReadLine() |> ignore
        let playerdamage = Renderer.renderSlotSpinning gamestate.Player.Reels |> GameLogic.calcuDamage gamestate.Player gamestate.CurrentEnemy 
        gamestate.CurrentEnemy.Hp <- gamestate.CurrentEnemy.Hp - int(playerdamage)
        printfn "Player attacks! Dealt [%d] damage. (Enemy HP left: %d)" (int playerdamage) (max 0 gamestate.CurrentEnemy.Hp)
        if gamestate.CurrentEnemy.Hp > 0 then
            gamestate.Player.Hp <- gamestate.Player.Hp - gamestate.CurrentEnemy.Damage
            printfn "Enemy counter-attacks! You took [%d] damage. (Your HP left: %d)" gamestate.CurrentEnemy.Damage (max 0 gamestate.Player.Hp)
            if gamestate.Player.Hp > 0 then
                roundPlay gamestate
            else
                GameLogic.gameOver gamestate.Round 
        else
            if gamestate.Player.Hp > 0 then
                printfn "Victory! The enemy has been defeated."
                
                if gamestate.Round = 10 then
                    printfn "\n--- [End of Round %d] ---" gamestate.Round
                    printfn "\n===================================="
                    printfn "Well done! You've cleared all rounds."
                    printfn "===================================="
                else
                    GameLogic.getReward gamestate
                    printfn "\n--- [End of Round %d] ---" gamestate.Round
                    printfn "Moving on to the next round...\n"
                    if gamestate.Round % 3 = 0 then
                        let rand = Random ()
                        let nextElement = 
                            match gamestate.RoundElement with
                            | Fire -> [Water;Grass].[rand.Next(2)]
                            | Water -> [Fire;Grass].[rand.Next(2)]
                            | Grass -> [Water;Fire].[rand.Next(2)]
                            | Prism -> failwith "Error"
                        roundPlay { gamestate with Round = gamestate.Round + 1; CurrentEnemy = GameLogic.createEnemy (gamestate.Round + 1) nextElement ; RoundElement = nextElement }
                    elif gamestate.Round = 10 then
                        printfn "The next boss is the most powerful enemy. It takes 1x damage from all attributes, but 1.5x damage from Prism attributes."
                        roundPlay { gamestate with Round = gamestate.Round + 1; CurrentEnemy = GameLogic.createEnemy (gamestate.Round + 1) Prism ; RoundElement = Prism }
                    else
                        roundPlay { gamestate with Round = gamestate.Round + 1; CurrentEnemy = GameLogic.createEnemy (gamestate.Round + 1) gamestate.RoundElement }
                        
                        
            else
                printfn "\n Game Over... You have fallen in battle."
                GameLogic.gameOver gamestate.Round 



    // 유저 프린트
    Renderer.printinitial initialReels
    roundPlay initialState
    Thread.Sleep(1000)
    0