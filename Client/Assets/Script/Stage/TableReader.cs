using UnityEngine;
using DataTable;


public class TableReader
{
    public string tablePath { get; private set; }
    public LOAD_MODE loadMode { get; private set; }

    private NiceTable _table = null;
    private bool alreadyRead = false;

    public TableReader(string path)
        : this(path, LOAD_MODE.UNICODE)
    { }

    public TableReader(string path, LOAD_MODE loadMode)
    {
        this.tablePath = path;
        this.loadMode = loadMode;
    }

    public void Load()
    {
        alreadyRead = true;
        _table = GameCommon.LoadTable(tablePath, loadMode);
    }

    public NiceTable table
    {
        get
        {
            if (!alreadyRead)
            {
                alreadyRead = true;
                _table = GameCommon.LoadTable(tablePath, loadMode);
            }

            return _table;
        }
    }
}