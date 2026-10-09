namespace DanishCvr.Models;

/// <summary>
/// Virksomheds Form.
/// </summary>
public class VirksomhedsForm : DatoOgPeriode
{
    /// <summary>
    /// Virksomheds Form Kode.
    /// </summary>
    public virtual int VirksomhedsFormKode { get; set; }

    /// <summary>
    /// Kort Beskrivelse.
    /// </summary>
    public virtual string KortBeskrivelse { get; set; }

    /// <summary>
    /// Lang Beskrivelse.
    /// </summary>
    public virtual string LangBeskrivelse { get; set; }

    /// <summary>
    /// Ansvarlig Data Leverandoer.
    /// </summary>
    public virtual string AnsvarligDataLeverandoer { get; set; }
}