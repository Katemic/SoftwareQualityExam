using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;

namespace LibraryAPI.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;

        public ItemService(IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        

        public async Task<IEnumerable<ItemDto>> GetAllAsync()
        {
            var items = await _itemRepository.GetAllAsync();

            return items.Select(i => new ItemDto
            {
                Id = i.Id,
                Name = i.Name,
                ReleaseYear = i.ReleaseYear,
                MediaType = i.MediaType,
                AverageStars = i.AverageStars
            });
        }

        public async Task<ItemDetailsDto?> GetByIdAsync(int id)
        {
            var item = await _itemRepository.GetByIdAsync(id);
            if (item == null)
                return null;

            var itemDto = new ItemDetailsDto
            {
                Id = item.Id,
                Name = item.Name,
                ReleaseYear = item.ReleaseYear,
                Description = item.Description,
                ReviewSummary = item.ReviewSummary,
                MediaType = item.MediaType,
                Image = item.Image,
                AverageStars = item.AverageStars,
                Language = item.Language?.Language1,
                Publisher = item.Publisher?.Name,
                Creators = item.Creators.Select(c => $"{c.FirstName} {c.LastName}").ToList(),
                Genres = item.Genres.Select(g => g.Name ?? "").ToList(),
                Tags = item.Tags.Select(t => t.Name ?? "").ToList()
            };
                if (item.MediaType == "book")
            {
                itemDto.BookDetails = new BookDto
                {
                    Isbn = item.Book?.Isbn,
                    NoOfPages = item.Book?.NoOfPages,
                    Version = item.Book?.Version
                };
            }
            else if (item.MediaType == "boardgame")
            {
                itemDto.BoardgameDetails = new BoardgameDto
                {
                    NoOfPlayers = item.Boardgame?.NoOfPlayers,
                    PlayTime = item.Boardgame?.PlayTime,
                    AgeGroup = item.Boardgame?.AgeGroup
                };
            }
            return itemDto;
        }

        public async Task<ItemDetailsDto> AddAsync(CreateItemDto itemDto)
        {
            var creators = await _itemRepository.GetCreatorsByIdsAsync(itemDto.CreatorIds);
            var genres = await _itemRepository.GetGenresByIdsAsync(itemDto.GenreIds);
            var tags = await _itemRepository.GetTagsByIdsAsync(itemDto.TagIds);

            var item = new Item
            {
                Name = itemDto.Name,
                ReleaseYear = (short?)itemDto.ReleaseYear,
                Description = itemDto.Description,
                MediaType = itemDto.MediaType,
                Image = itemDto.Image,
                LanguageId = (int)itemDto.LanguageId,
                PublisherId = (int)itemDto.PublisherId,

                Creators = creators,
                Genres = genres,
                Tags = tags,

                Book = itemDto.MediaType == "Book" && itemDto.Book != null
            ? new Book
            {
                Isbn = itemDto.Book.Isbn,
                NoOfPages = (short?)itemDto.Book.NoOfPages,
                Version = itemDto.Book.Version
            }
            : null,

                Boardgame = itemDto.MediaType == "Boardgame" && itemDto.Boardgame != null
            ? new Boardgame
            {
                NoOfPlayers = itemDto.Boardgame?.NoOfPlayers,
                PlayTime = itemDto.Boardgame?.PlayTime,
                AgeGroup = itemDto.Boardgame?.AgeGroup
            }
            : null
            };

            var createdItem = await _itemRepository.AddAsync(item);

            return await GetByIdAsync(createdItem.Id)
                ?? throw new Exception("Created item could not be found.");
        }

        public async Task<bool> UpdateAsync(int id, UpdateItemDto dto)
        {
            var existingItem = await _itemRepository.GetByIdAsync(id);

            if (existingItem == null)
            {
                return false;
            }

            var mediaType = dto.MediaType?.ToLower();

            existingItem.Name = dto.Name;
            existingItem.ReleaseYear = (short?)dto.ReleaseYear;
            existingItem.Description = dto.Description;
            existingItem.MediaType = mediaType;
            existingItem.Image = dto.Image;

            if (dto.LanguageId.HasValue)
            {
                existingItem.LanguageId = dto.LanguageId.Value;
            }

            if (dto.PublisherId.HasValue)
            {
                existingItem.PublisherId = dto.PublisherId.Value;
            }

            var creators = await _itemRepository.GetCreatorsByIdsAsync(dto.CreatorIds);
            var genres = await _itemRepository.GetGenresByIdsAsync(dto.GenreIds);
            var tags = await _itemRepository.GetTagsByIdsAsync(dto.TagIds);

            existingItem.Creators.Clear();
            foreach (var creator in creators)
            {
                existingItem.Creators.Add(creator);
            }

            existingItem.Genres.Clear();
            foreach (var genre in genres)
            {
                existingItem.Genres.Add(genre);
            }

            existingItem.Tags.Clear();
            foreach (var tag in tags)
            {
                existingItem.Tags.Add(tag);
            }

            if (mediaType == "book")
            {
                if (existingItem.Boardgame != null)
                {
                    _itemRepository.RemoveBoardgame(existingItem.Boardgame);
                    existingItem.Boardgame = null;
                }

                if (existingItem.Book == null)
                {
                    existingItem.Book = new Book();
                }

                existingItem.Book.Isbn = dto.Book?.Isbn;
                existingItem.Book.NoOfPages = (short?)(dto.Book?.NoOfPages);
                existingItem.Book.Version = dto.Book?.Version;
            }
            else if (mediaType == "boardgame")
            {
                if (existingItem.Book != null)
                {
                    _itemRepository.RemoveBook(existingItem.Book);
                    existingItem.Book = null;
                }

                if (existingItem.Boardgame == null)
                {
                    existingItem.Boardgame = new Boardgame();
                }

                existingItem.Boardgame.NoOfPlayers = dto.Boardgame?.NoOfPlayers;
                existingItem.Boardgame.PlayTime = dto.Boardgame?.PlayTime;
                existingItem.Boardgame.AgeGroup = dto.Boardgame?.AgeGroup;
            }

            return await _itemRepository.UpdateAsync(existingItem);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _itemRepository.DeleteAsync(id);
        }


    }
}
