using AutoMapper;
using KanbanBoard.Application.DTOs.Responses;
using KanbanBoard.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace KanbanBoard.Application.Mapping
{
    //Mapping profile for mapping the request to the domain entities and the domain entities to the response objects, using AutoMapper.
    public class KanbanMapping : Profile
    {
        public KanbanMapping()
        {
            // Board mappings
            CreateMap<Board, BoardResponseDto>()
                .ForMember(dest => dest.Columns, opt => opt.MapFrom(src => src.Columns));

            CreateMap<Board, BoardSummaryResponseDto>()
                .ForMember(dest => dest.ColumnCount, opt => opt.MapFrom(src => src.Columns.Count))
                .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Members.Count));

            // Column mappings
            CreateMap<Column, ColumnResponseDto>()
                .ForMember(dest => dest.Cards, opt => opt.MapFrom(src => src.Cards));

            // Card mappings
            CreateMap<Card, CardResponseDto>();

            // BoardMember mappings
            CreateMap<BoardMember, BoardMemberResponseDto>();
        }
    }
}
