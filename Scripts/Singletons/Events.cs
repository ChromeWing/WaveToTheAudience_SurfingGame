using System;

public class Events:Singleton<Events>{

	public event Action<SurfPlayer> performedTrick;

}