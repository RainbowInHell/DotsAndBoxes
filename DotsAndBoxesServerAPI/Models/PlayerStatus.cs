﻿namespace DotsAndBoxesServerAPI;

public enum PlayerStatus
{
    [ExtendedDisplayName("Доступен для совместной игры")]
    FreeToPlay,

    [ExtendedDisplayName("Недоступен для совместной игры")]
    DoNotDisturb,

    [ExtendedDisplayName("Приглашен другим игроком")]
    Challenged,

    [ExtendedDisplayName("Уже играет")]
    Playing
}