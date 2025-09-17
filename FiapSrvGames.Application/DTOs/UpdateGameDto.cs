using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapSrvGames.Application.DTOs;

[ExcludeFromCodeCoverage]
public class UpdateGameDto
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(100, ErrorMessage = "O título deve ter no máximo 100 caracteres.")]
    public string Title { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "O preço é obrigatório.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero.")]
    public double Price { get; set; }

    [Required(ErrorMessage = "A data de lançamento é obrigatória.")]
    [DataType(DataType.Date, ErrorMessage = "Data de lançamento inválida.")]
    public DateTime ReleaseDate { get; set; }
}
