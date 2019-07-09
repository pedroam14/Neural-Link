using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Interface for classes that provide methods to procedurally Generate new
 * {@link IMZDungeon}s.
 */
public interface IMZDungeonGenerator {

    /**
     * Generates a new {@link IMZDungeon}.
     */
    void Generate();
    
    /**
     * Gets the most recently Generated {@link IMZDungeon}.
     * 
     * @return the most recently Generated IMZDungeon
     */
    IMZDungeon GetMZDungeon();
    
}
