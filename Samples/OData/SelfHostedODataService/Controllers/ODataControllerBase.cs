﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;

namespace SelfHostedODataService.Controllers
{
  public abstract class ODataControllerBase<TEntity> : ODataController
    where TEntity : class, Joker.Domain.IDomainEntity
  {   
    #region Get

    [EnableQuery(MaxExpansionDepth = 3)]
    public OkObjectResult Get(ODataQueryOptions<TEntity> queryOptions)
    {
      var entities = GetAll();

      return Ok(entities);
    }

    protected abstract IQueryable<TEntity> GetAll();

    #endregion

    #region Post

    public async Task<IActionResult> Post([FromBody]TEntity entity)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      await OnPost(entity);

      return Created(entity);
    }

    protected abstract Task<int> OnPost(TEntity entity);

    #endregion

    public async Task<IActionResult> Patch(Delta<TEntity> entity)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var id = entity.GetInstance().Id;
      var entityToUpdate = GetAll().FirstOrDefault(c => c.Id == id);

      if (entityToUpdate == null)
        return NotFound();

      entity.Patch(entityToUpdate);

      var result = await OnPut(entityToUpdate);

      return Updated(entityToUpdate);
    }

    #region Put

    public async Task<IActionResult> Put([FromBody]TEntity entity)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      await OnPut(entity);

      return Updated(entity);
    }

    protected abstract Task<int> OnPut(TEntity entity);

    #endregion

    #region Delete

    public async Task<IActionResult> Delete([FromODataUri] int key)
    {
      var result = await OnDelete(key);

      return StatusCode((int)HttpStatusCode.NoContent);
    }

    #endregion

    #region OnDelete

    protected abstract Task<int> OnDelete(int key);

    #endregion
  }
}