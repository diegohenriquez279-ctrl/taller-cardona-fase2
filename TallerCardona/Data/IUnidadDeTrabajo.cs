namespace TallerCardona.Data;

// Unidad de trabajo: agrupa los cambios que hacen los repositorios
// y los guarda todos juntos en la base de datos con un solo SaveChanges.
public interface IUnidadDeTrabajo
{
    int Guardar();
}
